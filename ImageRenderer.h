﻿//------------------------------------------------------------------------------
// <copyright file="ImageRenderer.h" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// Manages the drawing of image data

#pragma once

#include <d2d1.h>
#include "NuiApi.h"
#include <math.h>
#include <dwrite.h>
#include "SpotMeBrain.h"

#pragma comment(lib, "Dwrite")


class ImageRenderer
{
public:

	//Constants
	static const int        cScreenWidth = 320;
	static const int        cScreenHeight = 240;

	bool					displayRecording;
	int						recordingDialogueValue;
	int skeletonAngle;


    /// <summary>
    /// Constructor
    /// </summary>
    ImageRenderer();

    /// <summary>
    /// Destructor
    /// </summary>
    virtual ~ImageRenderer();

    /// <summary>
    /// Set the window to draw to as well as the video format
    /// Implied bits per pixel is 32
    /// </summary>
    /// <param name="hWnd">window to draw to</param>
    /// <param name="pD2DFactory">already created D2D factory object</param>
    /// <param name="sourceWidth">width (in pixels) of image data to be drawn</param>
    /// <param name="sourceHeight">height (in pixels) of image data to be drawn</param>
    /// <param name="sourceStride">length (in bytes) of a single scanline</param>
    /// <returns>indicates success or failure</returns>
    HRESULT Initialize(HWND hwnd, ID2D1Factory* pD2DFactory, int sourceWidth, int sourceHeight, int sourceStride, CSpotMeBrain* brainPointer);

    /// <summary>
    /// Draws a 32 bit per pixel image of previously specified width, height, and stride to the associated hwnd
    /// </summary>
    /// <param name="pImage">image data in RGBX format</param>
    /// <param name="cbImage">size of image data in bytes</param>
    /// <returns>indicates success or failure</returns>
    HRESULT DrawColorImage(BYTE* pImage, unsigned long cbImage);

	HRESULT DrawFrame(BYTE* pImage, unsigned long cbImage, NUI_SKELETON_DATA skeletonData, short flags);

	//Draw Skeleton
	void DrawSkeleton(const NUI_SKELETON_DATA & skel, int windowWidth, int windowHeight);

	void DrawStaticSkeleton(const NUI_SKELETON_DATA & skel, int windowWidth, int windowHeight, int positionValue);

	//Draw Bone
	void DrawBone(const NUI_SKELETON_DATA & skel, NUI_SKELETON_POSITION_INDEX joint0, NUI_SKELETON_POSITION_INDEX joint1);

	D2D1_POINT_2F SkeletonToScreen(Vector4 skeletonPoint, int width, int height);

	void ProcessSkeleton(NUI_SKELETON_FRAME skeletonFrame);

	void DrawFrameStaticSkeleton(NUI_SKELETON_DATA skeletonFrame, int position, GRT::VectorFloat likelihoods);

	void DrawPositionText(int position, GRT::VectorFloat likelihoods);

	void drawRecordingDialogue(int dialogVal);

	CSpotMeBrain* recognitionBrain_p;

private:
    HWND                     m_hWnd;

    // Format information
    UINT                     m_sourceHeight;
    UINT                     m_sourceWidth;
    LONG                     m_sourceStride;

     // Direct2D 
    ID2D1Factory*            m_pD2DFactory;
	IDWriteFactory*          m_pWriteFactory;
    ID2D1HwndRenderTarget*   m_pRenderTarget;
    ID2D1Bitmap*             m_pBitmap;

	//Brushes
	ID2D1SolidColorBrush*    m_pBrushJointTracked;
	ID2D1SolidColorBrush*    m_pBrushJointHighlighted;
	ID2D1SolidColorBrush*    m_pBrushJointInferred;
	ID2D1SolidColorBrush*    m_pBrushBoneTracked;
	ID2D1SolidColorBrush*    m_pBrushBoneInferred;
	D2D1_POINT_2F            m_Points[NUI_SKELETON_POSITION_COUNT];

	IDWriteTextFormat*       m_pTextFormat;

	

	


	Vector4 RotateVector4AroundOrigin(Vector4 inVector, double yRotationAngle, double zRotationAngle);

	bool DrawPositionArrows(int positionValue);

    /// <summary>
    /// Ensure necessary Direct2d resources are created
    /// </summary>
    /// <returns>indicates success or failure</returns>
    HRESULT EnsureResources( );

    /// <summary>
    /// Dispose of Direct2d resources 
    /// </summary>
    void DiscardResources( );
};