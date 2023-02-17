// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H

// add headers that you want to pre-compile here
#include "framework.h"
/*
// macros
// Windows build
#if defined (_WIN32)
#if defined (CGALNATIVE_DLL_EXPORTS)
#define CGALNATIVE_CPP_CLASS __declspec(dllexport)
#define CGALNATIVE_CPP_FUNCTION __declspec(dllexport)
#define CGALNATIVE_C_FUNCTION extern "C" __declspec(dllexport)
#else
#define CGALNATIVE_CPP_CLASS __declspec(dllimport)
#define CGALNATIVE_CPP_FUNCTION __declspec(dllimport)
#define CGALNATIVE_C_FUNCTION extern "C" __declspec(dllimport)
#endif // CGALNATIVE_DLL_EXPORTS
#endif // _WIN32

#include <CGAL/Exact_predicates_inexact_constructions_kernel.h>
#include <CGAL/Polygon_with_holes_2.h>
#include <CGAL/create_straight_skeleton_from_polygon_with_holes_2.h>

typedef CGAL::Exact_predicates_inexact_constructions_kernel K;
typedef K::Point_2                    Point;
typedef CGAL::Polygon_2<K>            Polygon_2;
typedef CGAL::Polygon_with_holes_2<K> Polygon_with_holes;
typedef CGAL::Straight_skeleton_2<K>  Ss;

typedef boost::shared_ptr<Ss> SsPtr;
*/


#endif //PCH_H
