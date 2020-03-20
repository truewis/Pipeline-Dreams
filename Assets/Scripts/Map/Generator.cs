﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace PipelineDreams.Map
{
    public enum VertexAnnotation
    {
        MultiEntranceRoom, Room, Bridge, Door, Deadend

    }
    public struct VertexData
    {
        Vector3Int position;
        int index;

    }
    public abstract class GameStageSetup {
        //Stage generation parameters
        int NumStage;
        int StageGraphMaxDepth;
        /// <summary>
        /// Note that generators can restrict the number of connections.
        /// </summary>
        int NumAverageConnection;
        /// <summary>
        /// Determines the average size of stages. The number of features increases; the size stays the same.
        /// </summary>
        int StageSizeMultiplier;
        int gameSize;

        Graph<MapMetaData> Stages = new Graph<MapMetaData>();
        public void GenerateStages(int seed) {
           // Stages.MakeNewVertex();
        }
    }
    
    public abstract class Generator : ScriptableObject
    {
        public MapFeatData LastGenData { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="featsInBound"></param>
        /// <returns>(min, max)</returns>
        public Tuple<Vector3Int, Vector3Int> Boundary(IEnumerable<MapFeature> featsInBound) {
            var max = Vector3Int.zero;
            var min = Vector3Int.zero;
            foreach (var x in featsInBound)
                foreach (var v in x.OccupiedCells) {
                    var p = Vector3Int.RoundToInt(x.Rotation * v) + x.Position;
                    max.x = Math.Max(p.x, max.x);
                    max.y = Math.Max(p.y, max.y);
                    max.z = Math.Max(p.z, max.z);
                    min.x = Math.Min(p.x, min.x);
                    min.y = Math.Min(p.y, min.y);
                    min.z = Math.Min(p.z, min.z);
                }
            return new Tuple<Vector3Int, Vector3Int>(min, max);
        }
        public abstract MapFeatData GenerateMap(int seed, float scale = 1);
        /// <summary>
        /// </summary>
        /// <param name="featsToAvoid">Features to go around when generating paths. You should NOT include featsToConnect.</param>
        /// <param name="featsToConnect">n. If the features have designated entrances, paths are generated toward them and nothing else. If not, then paths are generated toward any point the feature occupies.</param>
        /// <param name="pathSimplicity">Paths are more likely to differ from the shortest path. =>0. </param>
        /// <param name="pathLinearity">Paths are less likely to be linear. >0. </param>
        /// <param name="pathWidth">The width of the path.</param>
        /// <param name="margin">The margin between the boundary of feats and the boundary in which the paths are generated. The former is included in the latter. >=1</param>
        /// <param name="prng">PRNG that returns [0,1)</param>
        public List<PDMapPath> GeneratePaths(IEnumerable<MapFeature> featsToAvoid, IEnumerable<MapFeature> featsToConnect, int pathSimplicity, int pathLinearity, int pathWidth, int margin, Func<float> prng) {
            var paths = new List<PDMapPath>();
            var AllFeats = featsToAvoid.ToList().Concat(featsToConnect);
            var boundsFeat = Boundary(AllFeats);
            var min = boundsFeat.Item1 - Vector3Int.one * margin;
            var max = boundsFeat.Item2 + Vector3Int.one * margin;
            var X = max.x - min.x;
            var Y = max.y - min.y;
            var Z = max.z - min.z;
            var n = featsToConnect.Count();
            var cvRadius = (pathWidth - 1) / 2;
            int[,,] DistanceGrid = new int[X, Y, Z];
            bool[,,] OccupancyGrid = new bool[X, Y, Z];
            float[] DistanceWeight = new float[] { 1 / (float)(1 + 0.4f*pathSimplicity), 1 /(1 + 0.2f*pathSimplicity), 1 };

            float[] LinearityWeight = new float[] { 1 / (float)(1 + pathLinearity), 1 };
            List<Vector3Int> ConnectedCells = new List<Vector3Int>();
            void SetGrid<T>(T[,,] grid, Vector3Int WorldPosition, T value) {
                grid[WorldPosition.x - min.x, WorldPosition.y - min.y, WorldPosition.z - min.z] = value;

            }
            T GetGrid<T>(T[,,] grid, Vector3Int WorldPosition) =>
                    grid[WorldPosition.x - min.x, WorldPosition.y - min.y, WorldPosition.z - min.z];
            bool GridExists<T>(T[,,] grid, Vector3Int WorldPosition) {
                try {
                    var a = grid[WorldPosition.x - min.x, WorldPosition.y - min.y, WorldPosition.z - min.z];
                }
                catch (IndexOutOfRangeException) {
                    return false;
                }
                return true;
            }
            T SelectRandom<T>(IEnumerable<T> selection) {
                var m = selection.Count();
                var rn = prng();

                return selection.ElementAt((int)(m * rn));
            }
            T SelectRandomWeight<T>(IEnumerable<T> selection, Func<T, float> weight) {
                float weightSum = 0;
                foreach (var x in selection)
                    weightSum += weight(x);
                var rn = prng();

                float rand = weightSum * rn;
                foreach (var x in selection) {
                    rand -= weight(x);
                    if (rand <= 0)
                        return x;
                }
                return selection.Last();
            }
            bool CheckOccupancy(Vector3Int center, int chebyshevRadius = 0) {
                for (int i = center.x - chebyshevRadius; i <= center.x + chebyshevRadius; i++)
                    for (int j = center.y - chebyshevRadius; j <= center.y + chebyshevRadius; j++)
                        for (int k = center.z - chebyshevRadius; k <= center.z + chebyshevRadius; k++)
                            if (GetGrid(OccupancyGrid, new Vector3Int(i, j, k)))
                                return true;

                return false;
            }
            //Fill the OccupancyGrid
            foreach (var x in AllFeats)
                foreach (var p in x.OccupiedCells)
                    SetGrid(OccupancyGrid, Vector3Int.RoundToInt(x.Rotation * p) + x.Position, true);


            //Assume that the entrances of the first room are "connected":
            var firstRoom = featsToConnect.First();
            if (firstRoom.Entrances.Count != 0)
                ConnectedCells.AddRange(from p in firstRoom.Entrances select Vector3Int.RoundToInt(firstRoom.Rotation * p.Position) + firstRoom.Position);
            else
                foreach (var p in firstRoom.OccupiedCells)
                    for (int f = 0; f < 6; f++) {
                        var vf = Vector3Int.RoundToInt(firstRoom.Rotation * p) + firstRoom.Position + Util.FaceToLHVector(f);
                        if (!GetGrid(OccupancyGrid, vf) && !ConnectedCells.Contains(vf))
                            ConnectedCells.Add(vf);
                    }

            for (int pathIndex = 1; pathIndex < n; pathIndex++) {



                //Build DistanceGrid from the connected cells.
                //Initialize the grid
                for (int i = 0; i < X; i++)
                    for (int j = 0; j < Y; j++)
                        for (int k = 0; k < Z; k++)
                            DistanceGrid[i, j, k] = -1;
                foreach (var x in ConnectedCells)
                    SetGrid(DistanceGrid, x, 0);

                //Calculate minimum distances: BFS
                Queue<Vector3Int> FrontierCells = new Queue<Vector3Int>(ConnectedCells);
                while (FrontierCells.Count > 0) {
                    var calc = FrontierCells.Dequeue();
                    //If this is the first time visiting calc:
                    int minDist = 0;

                    minDist = GetGrid(DistanceGrid, calc);
                    for (int f = 0; f < 6; f++) {
                        var vf = calc + Util.FaceToLHVector(f);
                        if (!GridExists(DistanceGrid, vf) || GetGrid(OccupancyGrid, vf))
                            continue;
                        var xf = GetGrid(DistanceGrid, vf);
                        if (xf == -1 || xf > minDist + 1) {
                            SetGrid(DistanceGrid, vf, minDist + 1);
                            FrontierCells.Enqueue(vf);
                        }

                    }
                }
                var path = new PDMapPath();
                MapFeature room = featsToConnect.ElementAt(pathIndex);
                DirectionalFeature addedEnt = new DirectionalFeature();
                //Set the starting point of the path to a random entrance of the room.
                //If the path starts in a feature, or the width of the path is large, we should "pull out" the path out of the feature first in order to prevent CheckOccupancy() from halting the generation.
                //To pull out, if there is a designated entrance, we move out of it. Otherwise, we move in random direction until the path does not overlap with the room
                if (room.Entrances.Count != 0) {
                    var ent = SelectRandom(room.Entrances);
                    var uv = Util.LHQToLHUnitVector(ent.Rotation* room.Rotation);
                    path.Head = Vector3Int.RoundToInt(room.Rotation * ent.Position) + room.Position;
                    room.UsedEntrances.Add(ent);
                    addedEnt = ent;
                } else {
                    var f = SelectRandom(new int[] { 0, 1, 2, 3, 4, 5 });
                    var uv = Util.FaceToLHVector(f);
                    Vector3Int p = Vector3Int.RoundToInt(room.Rotation * SelectRandom(room.OccupiedCells)) + room.Position;
                    while (GetGrid(OccupancyGrid, p)) {

                        p = uv + p;
                        path.Head = p;


                    }
                    //New Entrance is created!
                    addedEnt = new DirectionalFeature() { Position = p - room.Position, Rotation = Quaternion.Inverse(room.Rotation) * Util.FaceToLHQ(Util.FaceFlip(f)) };
                    room.UsedEntrances.Add(addedEnt);
                }
                path.Cells.Add(path.Head);
                //Distance Gradient Descent; with some exploration
                while (GetGrid(DistanceGrid, path.Cells.Last()) > 0) {
                    var availableNextCells = new List<Vector3Int>();
                    for (int f = 0; f < 6; f++) {
                        var p = path.Cells.Last() + Util.FaceToLHVector(f);
                        if (!GridExists(DistanceGrid, p))
                            continue;
                        //If p is safely distanced from anything dangerous
                        if (!CheckOccupancy(p, cvRadius))
                            availableNextCells.Add(p);
                    }

                    Vector3Int selected = Vector3Int.zero;
                    //Test the linearity.
                    if (path.Cells.Count < 2)
                        selected = SelectRandomWeight(availableNextCells, (x) => DistanceWeight[-GetGrid(DistanceGrid, x) + GetGrid(DistanceGrid, path.Cells.Last()) + 1]);
                    else {
                        var u = path.Cells.Last() - path.Cells[path.Cells.Count - 2];
                        selected = SelectRandomWeight(availableNextCells, (x) => DistanceWeight[-GetGrid(DistanceGrid, x) + GetGrid(DistanceGrid, path.Cells.Last()) + 1] * LinearityWeight[(x - path.Cells.Last() == u ? 1 : 0)]);
                    }
                    //If a loop is formed, delete the loop.
                    if (path.Cells.Contains(selected)) {
                        var i = path.Cells.IndexOf(selected);
                        path.Cells.RemoveRange(i, path.Cells.Count - i);
                    }
                    path.Cells.Add(selected);

                }
                //Add the tail to the path
                path.Tail = path.Cells.Last();


                //Add UsedEntrance to the features just connected at the tail;
                for (int f = 0; f < 6; f++)
                    foreach (var x in featsToConnect)
                        // for rooms do not have fixed entrances.
                        if (x.Entrances.Count == 0)
                        {
                            if (x.OccupiedCells.Contains(Vector3Int.RoundToInt(Quaternion.Inverse(x.Rotation) * (path.Tail + Util.FaceToLHVector(f) - x.Position)))
                            && !x.UsedEntrances.Any((ent) => ent.Position == Vector3Int.RoundToInt(Quaternion.Inverse(x.Rotation) * (path.Tail - x.Position)) && Util.LHQToFace(x.Rotation * ent.Rotation) == f))
                                x.UsedEntrances.Add(new DirectionalFeature() { Position = Vector3Int.RoundToInt(Quaternion.Inverse(x.Rotation) * (path.Tail - x.Position)), Rotation = Quaternion.Inverse(x.Rotation) * Util.FaceToLHQ(f) });
                        }
                        //for rooms of which entrances are fixed
                        else
                        {
                            if (x.Entrances.Any((ent) => ent.Position == Vector3Int.RoundToInt(Quaternion.Inverse(x.Rotation) * (path.Tail - x.Position)) && Util.LHQToFace(x.Rotation * ent.Rotation) == f)
                                && !x.UsedEntrances.Any((ent) => ent.Position == Vector3Int.RoundToInt(Quaternion.Inverse(x.Rotation) * (path.Tail - x.Position)) && Util.LHQToFace(x.Rotation * ent.Rotation) == f))
                                x.UsedEntrances.Add(new DirectionalFeature() { Position = Vector3Int.RoundToInt(Quaternion.Inverse(x.Rotation) * (path.Tail - x.Position)), Rotation = Quaternion.Inverse(x.Rotation) * Util.FaceToLHQ(f) });
                        }
                //Add joints to the paths connected at the tail:
                //If the path is singleton
                if (path.Cells.Count == 1) {
                    foreach (var path2 in paths)
                    {
                        for (int i = 1; i < path2.Cells.Count - 1; i++)
                            if (path2.Cells[i] == path.Tail)
                                path2.Joints.Add(new DirectionalFeature() { Position = path2.Cells[i], Rotation = addedEnt.Rotation });
                    }
                }
                else
                {
                    foreach (var path2 in paths)
                    {
                        for (int i = 1; i < path2.Cells.Count - 1; i++)
                            if (path2.Cells[i] == path.Tail)
                                path2.Joints.Add(new DirectionalFeature() { Position = path2.Cells[i], Rotation = Util.FaceToLHQ(Util.LHUnitVectorToFace(path.Cells[path.Cells.Count - 2] - path.Tail)) });
                    }
                    paths.Add(path);
                }
                //Refresh ConnectedCells
                ConnectedCells.AddRange(path.Cells);
                if (room.Entrances.Count != 0)
                    ConnectedCells.AddRange(from p in room.Entrances select Vector3Int.RoundToInt(room.Rotation * p.Position) + room.Position);
                else
                    foreach (var p in room.OccupiedCells)
                        for (int f = 0; f < 6; f++) {
                            var vf = Vector3Int.RoundToInt(room.Rotation * p) + room.Position + Util.FaceToLHVector(f);
                            if (!GetGrid(OccupancyGrid, vf) && !ConnectedCells.Contains(vf))
                                ConnectedCells.Add(vf);
                        }
            }

            return paths;
        }


    }
    public struct MapMetaData
    {
        int seed;
        Generator generator;
        Renderer renderer;
    }
    public class MapFeatData {
        public List<MapFeature> Features = new List<MapFeature>();
        public List<PDMapPath> Paths = new List<PDMapPath>();
    }
    public class MapFeature {
       
        public string Name;

        /// <summary>
        /// When multiple instances of a same feature exists, a unique index is given to each of them. 
        /// </summary>
        public int Index;
        /// <summary>
        /// The position of the feature origin.
        /// </summary>
        public Vector3Int Position;
        /// <summary>
        /// The position of the cells that this feature occupies; relative to the feature origin.
        /// </summary>
        public List<Vector3Int> OccupiedCells = new List<Vector3Int>();
        /// <summary>
        /// The position of entrances to the feature; if not specified, then every occupied point could be an entrance. All specified entrances should NOT be in OccupiedCells, and point toward an OccupiedCell.
        /// Multiple entrances could exist in a cell if they all point to different points.
        /// </summary>
        public List<DirectionalFeature> Entrances = new List<DirectionalFeature>();

        public List<DirectionalFeature> UsedEntrances = new List<DirectionalFeature>();
        public Quaternion Rotation = Quaternion.identity;
    }
    public class DirectionalFeature {
        public Vector3Int Position;
        public Quaternion Rotation;
    }
    public class PDMapPath {
        public Vector3Int Head;
        public Vector3Int Tail;
        /// <summary>
        /// The points where the path branches out
        /// </summary>
        public List<DirectionalFeature> Joints = new List<DirectionalFeature>();
        public List<Vector3Int> Cells = new List<Vector3Int>();
    }
    public class SquareRoom : MapFeature {
        public SquareRoom(int size) : base() {
            Name = "Room";
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    for (int k = 0; k < size; k++) {
                        OccupiedCells.Add(new Vector3Int(i, j, k));
                    }
        }
    }

    public class DeadendFeature : MapFeature
    {
        public DeadendFeature() : base() {
            Name = "Deadend";
            OccupiedCells.Add(Vector3Int.zero);
            //The entrance to this feature is at -z direction, heading +z direction.
            Entrances.Add(new DirectionalFeature() { Position = new Vector3Int(0,0,-1), Rotation = Util.FaceToLHQ(4)});
        }
    }
}