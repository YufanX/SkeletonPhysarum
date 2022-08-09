# SkeletonPhysarum
Skeleton Physarum - A Slime-Mold System Driven by Skeletonization Errors/ C# source code for Grasshopper Plugin "Slime"

My code project (2020-2022) "A Slime-Mold System Driven by Skeletonization Errors" on CDRF 2022 conference (Yufan Xie, Yufang Zhou, Jingsen Lian)

Intro:
This paper proposed a new way to generate slime mold patterns using a typical voronoi-based skeletonization method.
As a recursive system, it redraws and expands the resulting trails of skeletonization and feeds them back as an image source for skeletonization.
Through iterations, it utilizes the difference before and after skeletonization to generate slime-mold-like patterns.
During the whole process, we tested different growth types with different parameter settings and environmental conditions.

Since most researches on skeletonization focus on minimizing errors, on the opposite side this method utilizes errors of skeletonization
(e.g. subtracted skeletons at "branch" areas of the bitmap are different from the original brush trails or the best result we expect)
as the basis of the generative process.
The redraw process makes it possible to reconnect skeletons via intersected brushes, continuously changing the topology of the network.
Unlike the traditional slime mold algorithm which operates on every single agent, our method is driven by image-based solutions.
On the output side, this system provides a condensed vector result, which is more applicable for design purposes.

Video Link: https://youtu.be/Hv_NKE5N9BU
