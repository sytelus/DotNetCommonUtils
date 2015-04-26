using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils
{
    /// <summary>
    /// Implements Hopcroft Karp algorithm for finding maximum matching for unweighted undirected bipartiate graphs
    /// https://en.wikipedia.org/wiki/Hopcroft%E2%80%93Karp_algorithm
    /// </summary>
    public static class HopcroftKarp
    {
        /// <summary>
        /// Finds maximum matching for unweighted bipartiate graph in O(E sqrt(v)).
        /// The Hopcroft Karp algorithm is as follows:
        /// 1.  Augmented path has end points as two unmatched vertices. 
        ///     There may be 0 or more other vertices in the path besides source and destination however they must all be matched vertices.
        ///     The edges in the path must alternate between unmatched edge and matched edge. As the first vertice in path is unmatched, first edge is
        ///     automatically unmatched. If next vertex is unmatched as well then path ends otherwise we are on matched vertex in which case we must follow
        ///     edge from previous matching. After we follow the matched edge, we would be on other side but now we can not have any other matched edge so 
        ///     automatically, next edge must be unmatched to go on opposite side. And so the path continues.
        /// 2. Shortest augmented path is just that: Shortest in length of all augmented paths in graph.
        /// 3. There may be multiple shortest augmented paths of the same length.
        /// 4. Hopcroft Karp algorithm requires that we construct the maximal set of shortest augmented paths that don't have any common vertex between them.
        /// 5. Then so symmetric difference of existing matching and all augmented paths to get the new matching.
        /// 6. Repeat until no more augmented paths are found.
        /// 
        /// How do we do #4?
        /// 
        /// Let our graph be two partitions U, V. The key idea is to add a dummy vertex vDummy and connect it to unmatched vertices in V. Similarly add uDummy and 
        /// connect it to all unmatched vertices in U. Now if we run BFS from uDummy to vDummy then we can get shortest path between an unmatched vertex in U to unmatched vertex in V. 
        /// Due to bi-partiate nature of the graph, this path would zig zag from U to V. However we need to make sure that when going from V to U, we always select matched edge. If there
        /// is no matched edge then we abandon further search on that path and continue exploring other paths. If we make sure of this one condition then it meets all criteria for path being an
        /// augmented path as well as the shortest path.
        /// 
        /// Once we have found the shortest path, we want to make sure we ignore any paths that are longer than this shortest paths. BFS algorithm marks nodes in path with
        /// distance with source being 0. Thus, after doing BFS, we can start at each unmatched vertex in U, follow the path by following nodes with distance that increments by 1. 
        /// By the the time we arrive at destination vDummy, if its distance is 1 more than last node in V then we know that the path we followed is the shortest path. In that case,
        /// we just update the matching for each vertex on path in U and V. Note that each vertex in V on path, except for the last one, is matched vertex. 
        /// So updating this matching starting from first unmatched vertex is equivalent to removing previously matched edge and adding new unmatched edge in matching. This is same
        /// is symmetric difference (i.e. remove edges common to previous matching and add non-common edges in augmented path in new matching).
        /// 
        /// How do we make sure augmented paths are vertex disjoint? After we consider each augmented path, reset distance of vertices in path (that was originally set by BFS)
        /// to infinity. This prevents any other augmenting path with same vertice(s) getting considered.
        /// 
        /// Finally, we actually don't need uDummy because it's there just to put all unmatched vertices in queue when BFS starts. That we can do as just initialization. The 
        /// vDummy can be appened in U for convinience however we don't really need to add edges from V to vDummy. Instead, we can just initialize default pairing for all V
        /// to point to vDummy. That way, if final vertex in V doesn't have any matching vertex in U then we end at vDummy which is end of our augmented path. In below code,
        /// vDummy is denoted as iNil.
        /// 
        /// </summary>
        /// <param name="gU">Left vertices, each with edges to right vertices</param>
        /// <param name="gVCount">Count of right side of vertices</param>
        /// <returns>For each left vertice, corresponding ID for the matched right vertice</returns>
        public static IList<int> GetMatching(IList<int[]> gU, int gVCount)
        {
            //Recreate gV with dummy NIL node
            gU = new List<int[]>(gU);
            gU.Add(new int[] {});

            //Index of dummy NIL node
            var iNil = gU.Count - 1;
            var gUCount = iNil;

            //Create lists that would hold matching, by default they point to NIL
            var pairU = Enumerable.Repeat(iNil, gUCount).ToList();
            var pairV = Enumerable.Repeat(iNil, gVCount).ToList();
            var dist = new int[gU.Count];
            var q = new int[gU.Count];

            while (Bfs(gU, pairU, pairV, dist, iNil, gUCount, q))
            {
                for (var u = 0; u < gUCount; ++u)
                {
                    if (pairU[u] == iNil)
                        Dfs(gU, pairU, pairV, dist, iNil, u);
                }
            }

            return pairU.Select(i => i == iNil ? -1 : i).Take(gUCount).ToList();
        }

        private static bool Bfs(IList<int[]> gU, IReadOnlyList<int> pairU, IReadOnlyList<int> pairV, IList<int> dist, int iNil, int gUCount, IList<int> q)
        {
            int qiEnqueue = 0, qiDequeue = 0;
            for (var u = 0; u < gUCount; ++u)
            {
                if (pairU[u] == iNil)
                {
                    dist[u] = 0;
                    q[qiEnqueue++] = u;
                }
                else
                    dist[u] = int.MaxValue;
            }
            dist[iNil] = int.MaxValue;

            while (qiDequeue < qiEnqueue)
            {
                var u = q[qiDequeue++];
                if (dist[u] < dist[iNil])
                {
                    foreach (var v in gU[u])
                    {
                        var nextU = pairV[v];
                        if (dist[nextU] == int.MaxValue)
                        {
                            dist[nextU] = dist[u] + 1;
                            q[qiEnqueue++] = nextU;
                        }
                    }
                }
            }

            return dist[iNil] != int.MaxValue;
        }

        private static bool Dfs(IList<int[]> gU, IList<int> pairU, IList<int> pairV, IList<int> dist, int iNil, int u)
        {
            if (u != iNil)
            {
                foreach (var v in gU[u])
                {
                    var nextU = pairV[v];
                    if (dist[nextU] == dist[u] + 1)
                    {
                        if (Dfs(gU, pairU, pairV, dist, iNil, nextU))
                        {
                            pairU[u] = v;
                            pairV[v] = u;
                            return true;
                        }
                    }
                }
                dist[u] = int.MaxValue;
                return false;
            }
            return true;
        }

    }
}
