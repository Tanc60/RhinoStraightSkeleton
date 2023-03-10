#include "pch.h"
#include<boost/shared_ptr.hpp>
#include<CGAL/Exact_predicates_inexact_constructions_kernel.h>
#include<CGAL/Polygon_with_holes_2.h>
#include<CGAL/create_straight_skeleton_from_polygon_with_holes_2.h>
#include<CGAL/Polygon_2.h>
#include<CGAL/create_offset_polygons_2.h>



typedef CGAL::Exact_predicates_inexact_constructions_kernel K;

typedef K::Point_2                    Point;
typedef CGAL::Polygon_2<K>            Polygon_2;
typedef CGAL::Polygon_with_holes_2<K> Polygon_with_holes;
typedef CGAL::Straight_skeleton_2<K>  Ss;

typedef boost::shared_ptr<Ss> SsPtr;

typedef Ss::Vertex_const_handle		Vertex_const_handle;
typedef Ss::Halfedge_const_handle	Halfedge_const_handle;
typedef Ss::Halfedge_const_iterator	Halfedge_const_iterator;

typedef boost::shared_ptr<Polygon_2> PolygonPtr;
typedef std::vector<PolygonPtr> PolygonPtrVector;

struct Poly
{
	float* vertices;
	int verticesCount;
};

Polygon_2 CreatePolygon(float* vertices, int length)
{
	Polygon_2 result;
	for (int i = 0; i < length; i++)
	{
		//Get x and y elements
		float x = vertices[i * 2];
		float y = vertices[i * 2 + 1];

		//Create point
		result.push_back(Point(x, y));
	}

	return result;
}

struct SkeletonHandle
{
	SsPtr Skeleton;

	SkeletonHandle(SsPtr ptr)
	{
		Skeleton = ptr;
	}
};


extern "C" __declspec(dllexport) void ReleaseDoubleArray(double* arr)
{
	delete[] arr;
}

extern "C" __declspec(dllexport) void* GenerateStraightSkeleton(Poly * outer, Poly * holes, int holesCount, Poly * outStraightSkeleton, Poly * outSpokes)
{
	//Construct outer polygon (no holes yet)
	Polygon_with_holes poly(CreatePolygon(outer->vertices, outer->verticesCount));

	//Add holes
	for (int h = 0; h < holesCount; h++)
		poly.add_hole(CreatePolygon(holes[h].vertices, holes[h].verticesCount));

	//Construct skeleton
	SsPtr iss = CGAL::create_interior_straight_skeleton_2(poly);
	Ss& ss = *iss;

	//Create skeleton result (large enough to hold *all* vertices, we'll trim it later)
	outStraightSkeleton->verticesCount = ss.size_of_halfedges() * 2 * 2;	// edges * 2 (start and end) * 2 (X and Y)
	outStraightSkeleton->vertices = new float[outStraightSkeleton->verticesCount];

	//Create spoke result  (large enough to hold *all* vertices, we'll trim it later)
	outSpokes->verticesCount = ss.size_of_halfedges() * 2 * 2;	// edges * 2 (start and end) * 2 (X and Y)
	outSpokes->vertices = new float[outSpokes->verticesCount];

	//Copy vertex pairs
	int ssIndex = 0;
	int bsIndex = 0;
	for (Halfedge_const_iterator i = ss.halfedges_begin(); i != ss.halfedges_end(); ++i)
	{
		auto start = i->vertex();
		auto end = i->opposite()->vertex();

		if (!i->is_bisector())
			continue;

		auto startPos = start->point();
		auto endPos = end->point();

		auto startSkele = start->is_skeleton();
		auto endSkele = end->is_skeleton();

		if (startSkele && endSkele && i->is_inner_bisector())
		{
			outStraightSkeleton->vertices[ssIndex * 4] = startPos.x();
			outStraightSkeleton->vertices[ssIndex * 4 + 1] = startPos.y();
			outStraightSkeleton->vertices[ssIndex * 4 + 2] = endPos.x();
			outStraightSkeleton->vertices[ssIndex * 4 + 3] = endPos.y();
			ssIndex++;
		}
		else
		{
			outSpokes->vertices[bsIndex * 4] = startPos.x();
			outSpokes->vertices[bsIndex * 4 + 1] = startPos.y();
			outSpokes->vertices[bsIndex * 4 + 2] = endPos.x();
			outSpokes->vertices[bsIndex * 4 + 3] = endPos.y();
			bsIndex++;
		}
	}

	//Set the sizes
	outStraightSkeleton->verticesCount = ssIndex * 4;
	outSpokes->verticesCount = bsIndex * 4;

	//Create a handle for the result (effectively "leak" the skeleton out to C#)
	auto handle = new SkeletonHandle(iss);
	return (void*)handle;
}

extern "C" __declspec(dllexport) void FreePolygonStructMembers(Poly * poly)
{
	if (poly != nullptr)
	{
		delete poly->vertices;

		poly->vertices = nullptr;
		poly->verticesCount = 0;
	}
}

extern "C" __declspec(dllexport) void FreeResultHandle(void* opaqueHandle)
{
	if (opaqueHandle != nullptr)
	{
		SkeletonHandle* handle = (SkeletonHandle*)opaqueHandle;
		delete handle;
	}
}
