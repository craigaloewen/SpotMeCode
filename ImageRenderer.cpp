//------------------------------------------------------------------------------
// <copyright file="ImageRenderer.cpp" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#include "stdafx.h"
#include "ImageRenderer.h"

static const float g_JointThickness = 3.0f;
static const float g_TrackedBoneThickness = 6.0f;
static const float g_InferredBoneThickness = 1.0f;


/// <summary>
/// Constructor
/// </summary>
ImageRenderer::ImageRenderer() : 
    m_hWnd(0),
    m_sourceWidth(0),
    m_sourceHeight(0),
    m_sourceStride(0),
    m_pD2DFactory(NULL), 
	m_pWriteFactory(NULL),
	m_pTextFormat(NULL),
    m_pRenderTarget(NULL),
	m_pBrushJointTracked(NULL),
	m_pBrushJointHighlighted(NULL),
	m_pBrushJointInferred(NULL),
	m_pBrushBoneTracked(NULL),
	m_pBrushBoneInferred(NULL),
    m_pBitmap(0),
	displayRecording(FALSE)
{
	ZeroMemory(m_Points, sizeof(m_Points));
	skeletonAngle = 0;
}

/// <summary>
/// Destructor
/// </summary>
ImageRenderer::~ImageRenderer()
{
    DiscardResources();
    SafeRelease(m_pD2DFactory);
}

/// <summary>
/// Ensure necessary Direct2d resources are created
/// </summary>
/// <returns>indicates success or failure</returns>
HRESULT ImageRenderer::EnsureResources()
{

	static const WCHAR msc_fontName[] = L"Verdana";
	static const FLOAT msc_fontSize = 50;
    HRESULT hr = S_OK;

    if (NULL == m_pRenderTarget)
    {
        D2D1_SIZE_U size = D2D1::SizeU(m_sourceWidth, m_sourceHeight);

        D2D1_RENDER_TARGET_PROPERTIES rtProps = D2D1::RenderTargetProperties();
        rtProps.pixelFormat = D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE);
        rtProps.usage = D2D1_RENDER_TARGET_USAGE_GDI_COMPATIBLE;

        // Create a hWnd render target, in order to render to the window set in initialize
        hr = m_pD2DFactory->CreateHwndRenderTarget(
            rtProps,
            D2D1::HwndRenderTargetProperties(m_hWnd, size),
            &m_pRenderTarget
            );

        if ( FAILED(hr) )
        {
            return hr;
        }

        // Create a bitmap that we can copy image data into and then render to the target
        hr = m_pRenderTarget->CreateBitmap(
            size, 
            D2D1::BitmapProperties( D2D1::PixelFormat( DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE) ),
            &m_pBitmap 
            );

        if ( FAILED(hr) )
        {
            SafeRelease(m_pRenderTarget);
            return hr;
        }


		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(0.27f, 0.75f, 0.27f), &m_pBrushJointTracked);
		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Red, 1.0f), &m_pBrushJointHighlighted);

		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Yellow, 1.0f), &m_pBrushJointInferred);
		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green, 1.0f), &m_pBrushBoneTracked);
		m_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Gray, 1.0f), &m_pBrushBoneInferred);

		hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory), reinterpret_cast<IUnknown**>(&m_pWriteFactory));

		if (FAILED(hr))
		{
			SafeRelease(m_pRenderTarget);
			return hr;
		}

		hr = m_pWriteFactory->CreateTextFormat(
			L"Arial",                // Font family name.
			NULL,                       // Font collection (NULL sets it to use the system font collection).
			DWRITE_FONT_WEIGHT_REGULAR,
			DWRITE_FONT_STYLE_NORMAL,
			DWRITE_FONT_STRETCH_NORMAL,
			48.0f,
			L"en-us",
			&m_pTextFormat
		);

		if (FAILED(hr))
		{
			SafeRelease(m_pRenderTarget);
			return hr;
		}
		
		hr = m_pTextFormat->SetTextAlignment(DWRITE_TEXT_ALIGNMENT_TRAILING);
		hr = m_pTextFormat->SetParagraphAlignment(DWRITE_PARAGRAPH_ALIGNMENT_CENTER);





    }

    return hr;
}

/// <summary>
/// Dispose of Direct2d resources 
/// </summary>
void ImageRenderer::DiscardResources()
{
    SafeRelease(m_pRenderTarget);
    SafeRelease(m_pBitmap);
}

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
HRESULT ImageRenderer::Initialize(HWND hWnd, ID2D1Factory* pD2DFactory, int sourceWidth, int sourceHeight, int sourceStride, CSpotMeBrain* brainPointer)
{
    if (NULL == pD2DFactory)
    {
        return E_INVALIDARG;
    }

    m_hWnd = hWnd;

    // One factory for the entire application so save a pointer here
    m_pD2DFactory = pD2DFactory;

    m_pD2DFactory->AddRef();

    // Get the frame size
    m_sourceWidth  = sourceWidth;
    m_sourceHeight = sourceHeight;
    m_sourceStride = sourceStride;

	recognitionBrain_p = brainPointer;

    return S_OK;
}

HRESULT ImageRenderer::DrawFrame(BYTE* pImage, unsigned long cbImage, NUI_SKELETON_DATA skeletonData, short flags) {

	HRESULT hr;

	// create the resources for this draw device
	// they will be recreated if previously lost
	hr = EnsureResources();

	if (FAILED(hr))
	{
		return hr;
	}


	m_pRenderTarget->BeginDraw();
	//m_pRenderTarget->Clear();

	
	//Color Image Section
	if (pImage != NULL) {
		// incorrectly sized image data passed in
		if (cbImage < ((m_sourceHeight - 1) * m_sourceStride) + (m_sourceWidth * 4))
		{
			return E_INVALIDARG;
		}

		

		// Copy the image that was passed in into the direct2d bitmap
		hr = m_pBitmap->CopyFromMemory(NULL, pImage, m_sourceStride);

		// Draw the bitmap stretched to the size of the window
		m_pRenderTarget->DrawBitmap(m_pBitmap);

		if (FAILED(hr))
		{
			return hr;
		}
	}

	if (flags == 1) {
		DrawSkeleton(skeletonData, m_sourceWidth, m_sourceHeight);

	}
	//End Draw
	hr = m_pRenderTarget->EndDraw();

	// Device lost, need to recreate the render target
	// We'll dispose it now and retry drawing
	if (hr == D2DERR_RECREATE_TARGET)
	{
		hr = S_OK;
		DiscardResources();
	}

	return hr;

	



}

void ImageRenderer::DrawFrameStaticSkeleton(NUI_SKELETON_DATA skeletonData, int position, GRT::VectorFloat likelihoods) {

	HRESULT hr;

	hr = EnsureResources();

	if (FAILED(hr))
	{
		return;
	}

	m_pRenderTarget->BeginDraw();
	m_pRenderTarget->Clear();

	///XXX
	//DrawPositionText(position, likelihoods);

	// Process Each Skeleton

	int width = m_sourceWidth; //imageRect.right;
	int height = m_sourceHeight; //imageRect.bottom;

	for (int i = 0; i < NUI_SKELETON_COUNT; ++i)
	{
		NUI_SKELETON_TRACKING_STATE trackingState = skeletonData.eTrackingState;

		if (NUI_SKELETON_TRACKED == trackingState)
		{
			// We're tracking the skeleton, draw it
			DrawStaticSkeleton(skeletonData, width, height, position);
			DrawPositionArrows(position);

		}
		else if (NUI_SKELETON_POSITION_ONLY == trackingState)
		{
			// we've only received the center point of the skeleton, draw that
			D2D1_ELLIPSE ellipse = D2D1::Ellipse(
				SkeletonToScreen(skeletonData.Position, width, height),
				g_JointThickness,
				g_JointThickness
			);

			m_pRenderTarget->DrawEllipse(ellipse, m_pBrushJointTracked);
		}
	}

	if (displayRecording) {

		drawRecordingDialogue(recordingDialogueValue);
	}




	hr = m_pRenderTarget->EndDraw();

	if (hr == D2DERR_RECREATE_TARGET)
	{
		hr = S_OK;
		DiscardResources();
	}

}

void ImageRenderer::drawRecordingDialogue(int dialogVal) {

	//DrawText 
	D2D1_RECT_F testRect;

	testRect.left = 0;
	testRect.right = 100;
	testRect.bottom = m_sourceHeight;
	testRect.top = m_sourceHeight - 75;


	WCHAR text[10];
	swprintf_s(text, sizeof(text) / sizeof(WCHAR) - 1, L"%d", dialogVal);
	UINT cch = (UINT)wcsnlen_s(text, 10);
	m_pRenderTarget->DrawText(text, cch, m_pTextFormat, testRect, m_pBrushJointTracked);

}

void ImageRenderer::DrawSkeleton(const NUI_SKELETON_DATA & skel, int windowWidth, int windowHeight)
{
	int i;

	for (i = 0; i < NUI_SKELETON_POSITION_COUNT; ++i)
	{

		m_Points[i] = SkeletonToScreen(skel.SkeletonPositions[i], windowWidth, windowHeight);
	}

	// Render Torso
	DrawBone(skel, NUI_SKELETON_POSITION_HEAD, NUI_SKELETON_POSITION_SHOULDER_CENTER);
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SPINE);
	DrawBone(skel, NUI_SKELETON_POSITION_SPINE, NUI_SKELETON_POSITION_HIP_CENTER);
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_RIGHT);

	// Left Arm
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_LEFT, NUI_SKELETON_POSITION_ELBOW_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_ELBOW_LEFT, NUI_SKELETON_POSITION_WRIST_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_WRIST_LEFT, NUI_SKELETON_POSITION_HAND_LEFT);

	// Right Arm
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_RIGHT, NUI_SKELETON_POSITION_ELBOW_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_ELBOW_RIGHT, NUI_SKELETON_POSITION_WRIST_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_WRIST_RIGHT, NUI_SKELETON_POSITION_HAND_RIGHT);

	// Left Leg
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_LEFT, NUI_SKELETON_POSITION_KNEE_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_KNEE_LEFT, NUI_SKELETON_POSITION_ANKLE_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_ANKLE_LEFT, NUI_SKELETON_POSITION_FOOT_LEFT);

	// Right Leg
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_RIGHT, NUI_SKELETON_POSITION_KNEE_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_KNEE_RIGHT, NUI_SKELETON_POSITION_ANKLE_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_ANKLE_RIGHT, NUI_SKELETON_POSITION_FOOT_RIGHT);

	D2D1_ELLIPSE ellipse2 = D2D1::Ellipse(SkeletonToScreen(skel.Position, windowWidth, windowHeight), g_JointThickness, g_JointThickness);
	m_pRenderTarget->DrawEllipse(ellipse2, m_pBrushJointTracked);

	// Draw the joints in a different color
	for (i = 0; i < NUI_SKELETON_POSITION_COUNT; ++i)
	{
		D2D1_ELLIPSE ellipse = D2D1::Ellipse(m_Points[i], g_JointThickness, g_JointThickness);

		

		if (skel.eSkeletonPositionTrackingState[i] == NUI_SKELETON_POSITION_INFERRED)
		{
			m_pRenderTarget->DrawEllipse(ellipse, m_pBrushJointInferred);
		}
		else if (skel.eSkeletonPositionTrackingState[i] == NUI_SKELETON_POSITION_TRACKED)
		{
			m_pRenderTarget->DrawEllipse(ellipse, m_pBrushJointTracked);
		}
	}
}

void ImageRenderer::DrawPositionText(int position, GRT::VectorFloat likelihoods) {

	//DrawText 
	D2D1_RECT_F testRect;

	testRect.left = m_sourceWidth - 100;
	testRect.right = m_sourceWidth;
	testRect.top = 0;
	testRect.bottom = 0;


	WCHAR text[10];
	swprintf_s(text, sizeof(text) / sizeof(WCHAR) - 1, L"%d", position);
	UINT cch = (UINT)wcsnlen_s(text, 10);
	m_pRenderTarget->DrawText(text, cch, m_pTextFormat, testRect, m_pBrushJointTracked);

	for (int i = 0; i < likelihoods.size(); i++) {
		
		testRect.top += 30;
		testRect.bottom += 30;

		int readValue = likelihoods[i] * 100;

		swprintf_s(text, sizeof(text) / sizeof(WCHAR) - 1, L"%d", readValue);
		cch = (UINT)wcsnlen_s(text, 10);
		m_pRenderTarget->DrawText(text, cch, m_pTextFormat, testRect, m_pBrushJointTracked);


	}



}

Vector4 ImageRenderer::RotateVector4AroundOrigin(Vector4 inVector, double yRotationAngle, double zRotationAngle) {

	Vector4 outputVector;

	double radianYAngle = yRotationAngle;
	double radianZAngle = zRotationAngle;

	outputVector.x = cos(radianYAngle)*inVector.x + sin(radianYAngle)*inVector.z;
	outputVector.y = inVector.y;
	outputVector.z = -sin(radianYAngle)*inVector.x + cos(radianYAngle)*inVector.z;

	inVector = outputVector;

	outputVector.x = cos(radianZAngle)*inVector.x - sin(radianZAngle) * inVector.y;
	outputVector.y = sin(radianZAngle)*inVector.x + cos(radianZAngle) * inVector.y;
	outputVector.z = outputVector.z;

	inVector = outputVector;
	radianYAngle = skeletonAngle * PI / 180;

	outputVector.x = cos(radianYAngle)*inVector.x + sin(radianYAngle)*inVector.z;
	outputVector.y = inVector.y;
	outputVector.z = -sin(radianYAngle)*inVector.x + cos(radianYAngle)*inVector.z;

	return outputVector;
}

void ImageRenderer::DrawStaticSkeleton(const NUI_SKELETON_DATA & skel, int windowWidth, int windowHeight, int positionValue)
{

	int i;
	double yAngle, zAngle;

	Vector4 shoulderLineVector;
	Vector4 shoulderLineVectorStored;

	shoulderLineVector.x = skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].x - skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].x;
	shoulderLineVector.y = skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].y - skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].y;
	shoulderLineVector.z = skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].z - skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].z;

	yAngle = atan2(shoulderLineVector.z, shoulderLineVector.x);

	shoulderLineVectorStored = shoulderLineVector;

	shoulderLineVector.x = cos(yAngle)*shoulderLineVectorStored.x + sin(yAngle)*shoulderLineVectorStored.z;
	shoulderLineVector.y = shoulderLineVectorStored.y;
	shoulderLineVector.z = -sin(yAngle)*shoulderLineVectorStored.x + cos(yAngle)*shoulderLineVectorStored.z;

	zAngle = atan2(shoulderLineVector.y,shoulderLineVector.x);



	for (i = 0; i < NUI_SKELETON_POSITION_COUNT; ++i)
	{

		Vector4 inPoint;

		inPoint.x = skel.SkeletonPositions[i].x - skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_CENTER].x;
		inPoint.y = skel.SkeletonPositions[i].y - skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_CENTER].y;
		inPoint.z = skel.SkeletonPositions[i].z - skel.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_CENTER].z;
		inPoint.w = skel.SkeletonPositions[i].w;

		inPoint = RotateVector4AroundOrigin(inPoint, yAngle,-zAngle);

		inPoint.z += 2.5;
		inPoint.y += 0.5;


		m_Points[i] = SkeletonToScreen(inPoint, windowWidth, windowHeight);
	}

	// Render Torso
	DrawBone(skel, NUI_SKELETON_POSITION_HEAD, NUI_SKELETON_POSITION_SHOULDER_CENTER);
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SHOULDER_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_CENTER, NUI_SKELETON_POSITION_SPINE);
	DrawBone(skel, NUI_SKELETON_POSITION_SPINE, NUI_SKELETON_POSITION_HIP_CENTER);
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_CENTER, NUI_SKELETON_POSITION_HIP_RIGHT);

	// Left Arm
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_LEFT, NUI_SKELETON_POSITION_ELBOW_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_ELBOW_LEFT, NUI_SKELETON_POSITION_WRIST_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_WRIST_LEFT, NUI_SKELETON_POSITION_HAND_LEFT);

	// Right Arm
	DrawBone(skel, NUI_SKELETON_POSITION_SHOULDER_RIGHT, NUI_SKELETON_POSITION_ELBOW_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_ELBOW_RIGHT, NUI_SKELETON_POSITION_WRIST_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_WRIST_RIGHT, NUI_SKELETON_POSITION_HAND_RIGHT);

	// Left Leg
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_LEFT, NUI_SKELETON_POSITION_KNEE_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_KNEE_LEFT, NUI_SKELETON_POSITION_ANKLE_LEFT);
	DrawBone(skel, NUI_SKELETON_POSITION_ANKLE_LEFT, NUI_SKELETON_POSITION_FOOT_LEFT);

	// Right Leg
	DrawBone(skel, NUI_SKELETON_POSITION_HIP_RIGHT, NUI_SKELETON_POSITION_KNEE_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_KNEE_RIGHT, NUI_SKELETON_POSITION_ANKLE_RIGHT);
	DrawBone(skel, NUI_SKELETON_POSITION_ANKLE_RIGHT, NUI_SKELETON_POSITION_FOOT_RIGHT);

	D2D1_ELLIPSE ellipse2 = D2D1::Ellipse(SkeletonToScreen(skel.Position, windowWidth, windowHeight), g_JointThickness, g_JointThickness);
	m_pRenderTarget->DrawEllipse(ellipse2, m_pBrushJointTracked);

	// Draw the joints in a different color
	for (i = 0; i < NUI_SKELETON_POSITION_COUNT; ++i)
	{
		D2D1_ELLIPSE ellipse = D2D1::Ellipse(m_Points[i], g_JointThickness, g_JointThickness);


		if (skel.eSkeletonPositionTrackingState[i] == NUI_SKELETON_POSITION_INFERRED)
		{
			m_pRenderTarget->DrawEllipse(ellipse, m_pBrushJointInferred);
		}
		else if (skel.eSkeletonPositionTrackingState[i] == NUI_SKELETON_POSITION_TRACKED)
		{
				m_pRenderTarget->DrawEllipse(ellipse, m_pBrushJointTracked);
		}
	}
}


bool ImageRenderer::DrawPositionArrows(int positionValue) {

	int checkValue = recognitionBrain_p->exerciseFileManager.currentExercise.numOfClassifications;

	if (positionValue > checkValue || positionValue == 0) {
		return false;
	}

	if (recognitionBrain_p->exerciseFileManager.currentExercise.arrowDirectionList[positionValue - 1] == 0) {
		if (recognitionBrain_p->exerciseFileManager.currentExercise.jointNumberList[positionValue - 1] == 0) {
			return false;
		} else {
			int jointNumber = recognitionBrain_p->exerciseFileManager.currentExercise.jointNumberList[positionValue - 1];
			m_pRenderTarget->DrawEllipse(D2D1::Ellipse(m_Points[jointNumber], g_JointThickness, g_JointThickness), m_pBrushJointHighlighted);
			if (recognitionBrain_p->exerciseFileManager.currentExercise.jointNumberList[positionValue - 1] > 3) {
				jointNumber += 4;
				m_pRenderTarget->DrawEllipse(D2D1::Ellipse(m_Points[jointNumber], g_JointThickness, g_JointThickness), m_pBrushJointHighlighted);
			}
		}
		return true;
	}


	ID2D1PathGeometry* p_arrow1Geometry;

	HRESULT hr = m_pD2DFactory->CreatePathGeometry(&p_arrow1Geometry);

	if (FAILED(hr)) {
		return false;
	}

	ID2D1GeometrySink *pSink = NULL;
	hr = p_arrow1Geometry->Open(&pSink);

	if (FAILED(hr)) {
		return false;
	}

	int jointNumber = recognitionBrain_p->exerciseFileManager.currentExercise.jointNumberList[positionValue - 1];

	int basex = m_Points[jointNumber].x;
	int basey = m_Points[jointNumber].y+5;

	pSink->SetFillMode(D2D1_FILL_MODE_WINDING);

	pSink->BeginFigure(
		D2D1::Point2F(basex, basey),
		D2D1_FIGURE_BEGIN_FILLED
	);
	D2D1_POINT_2F points[7] = {
		D2D1::Point2F(basex+10, basey+10),
		D2D1::Point2F(basex+5, basey+10),
		D2D1::Point2F(basex+5, basey+20),
		D2D1::Point2F(basex - 5, basey + 20),
		D2D1::Point2F(basex - 5, basey + 10),
		D2D1::Point2F(basex - 10, basey + 10),
		D2D1::Point2F(basex, basey),
	};
	pSink->AddLines(points, ARRAYSIZE(points));
	pSink->EndFigure(D2D1_FIGURE_END_CLOSED);
	hr = pSink->Close();

	float rotationNumber = (recognitionBrain_p->exerciseFileManager.currentExercise.arrowDirectionList[positionValue - 1]-1) * 90;

	ID2D1TransformedGeometry* p_ArrowFlipped;

	hr = m_pD2DFactory->CreateTransformedGeometry(
		p_arrow1Geometry,
		D2D1::Matrix3x2F::Rotation(
			rotationNumber,
			D2D1::Point2F(m_Points[jointNumber].x, m_Points[jointNumber].y)),
		&p_ArrowFlipped
	);
	m_pRenderTarget->FillGeometry(p_ArrowFlipped, m_pBrushJointInferred);
	m_pRenderTarget->DrawEllipse(D2D1::Ellipse(m_Points[jointNumber], g_JointThickness, g_JointThickness), m_pBrushJointHighlighted);

	// Draw Second Arrow if Applicable 

	if (recognitionBrain_p->exerciseFileManager.currentExercise.jointNumberList[positionValue - 1] > 3) {
		
		jointNumber += 4;
		basex = m_Points[jointNumber].x;
		basey = m_Points[jointNumber].y+5;

		ID2D1PathGeometry* p_arrow2Geometry;

		HRESULT hr = m_pD2DFactory->CreatePathGeometry(&p_arrow2Geometry);

		if (FAILED(hr)) {
			SafeRelease(pSink);
			return false;
		}

		ID2D1GeometrySink *pSink2 = NULL;
		hr = p_arrow2Geometry->Open(&pSink2);

		pSink2->SetFillMode(D2D1_FILL_MODE_WINDING);

		pSink2->BeginFigure(
			D2D1::Point2F(basex, basey),
			D2D1_FIGURE_BEGIN_FILLED
		);

		points[0] = D2D1::Point2F(basex + 10, basey + 10),
		points[1] = D2D1::Point2F(basex + 5, basey + 10),
		points[2] = D2D1::Point2F(basex + 5, basey + 20),
		points[3] = D2D1::Point2F(basex - 5, basey + 20),
		points[4] = D2D1::Point2F(basex - 5, basey + 10),
		points[5] = D2D1::Point2F(basex - 10, basey + 10),
		points[6] = D2D1::Point2F(basex, basey),
		pSink2->AddLines(points, ARRAYSIZE(points));
		pSink2->EndFigure(D2D1_FIGURE_END_CLOSED);
		hr = pSink2->Close();

		float rotationNumber = 0 - (recognitionBrain_p->exerciseFileManager.currentExercise.arrowDirectionList[positionValue - 1] - 1) * 90;

		ID2D1TransformedGeometry* p_ArrowFlipped2;

		hr = m_pD2DFactory->CreateTransformedGeometry(
			p_arrow2Geometry,
			D2D1::Matrix3x2F::Rotation(
				rotationNumber,
				D2D1::Point2F(m_Points[jointNumber].x, m_Points[jointNumber].y)),
			&p_ArrowFlipped2
		);
		m_pRenderTarget->FillGeometry(p_ArrowFlipped2, m_pBrushJointInferred);
		m_pRenderTarget->DrawEllipse(D2D1::Ellipse(m_Points[jointNumber], g_JointThickness, g_JointThickness), m_pBrushJointHighlighted);
		
		SafeRelease(pSink2);

	}




	SafeRelease(pSink);
	

	return true;

}


/// <summary>
/// Draws a bone line between two joints
/// </summary>
/// <param name="skel">skeleton to draw bones from</param>
/// <param name="joint0">joint to start drawing from</param>
/// <param name="joint1">joint to end drawing at</param>
void ImageRenderer::DrawBone(const NUI_SKELETON_DATA & skel, NUI_SKELETON_POSITION_INDEX joint0, NUI_SKELETON_POSITION_INDEX joint1)
{
	NUI_SKELETON_POSITION_TRACKING_STATE joint0State = skel.eSkeletonPositionTrackingState[joint0];
	NUI_SKELETON_POSITION_TRACKING_STATE joint1State = skel.eSkeletonPositionTrackingState[joint1];

	// If we can't find either of these joints, exit
	if (joint0State == NUI_SKELETON_POSITION_NOT_TRACKED || joint1State == NUI_SKELETON_POSITION_NOT_TRACKED)
	{
		return;
	}

	// Don't draw if both points are inferred
	if (joint0State == NUI_SKELETON_POSITION_INFERRED && joint1State == NUI_SKELETON_POSITION_INFERRED)
	{
		return;
	}

	// We assume all drawn bones are inferred unless BOTH joints are tracked
	if (joint0State == NUI_SKELETON_POSITION_TRACKED && joint1State == NUI_SKELETON_POSITION_TRACKED)
	{
		m_pRenderTarget->DrawLine(m_Points[joint0], m_Points[joint1], m_pBrushBoneTracked, g_TrackedBoneThickness);
	}
	else
	{
		m_pRenderTarget->DrawLine(m_Points[joint0], m_Points[joint1], m_pBrushBoneInferred, g_InferredBoneThickness);
	}
}

/// <summary>
/// Converts a skeleton point to screen space
/// </summary>
/// <param name="skeletonPoint">skeleton point to tranform</param>
/// <param name="width">width (in pixels) of output buffer</param>
/// <param name="height">height (in pixels) of output buffer</param>
/// <returns>point in screen-space</returns>
D2D1_POINT_2F ImageRenderer::SkeletonToScreen(Vector4 skeletonPoint, int width, int height)
{
	LONG x, y;
	USHORT depth;

	// Calculate the skeleton's position on the screen
	// NuiTransformSkeletonToDepthImage returns coordinates in NUI_IMAGE_RESOLUTION_320x240 space
	NuiTransformSkeletonToDepthImage(skeletonPoint, &x, &y, &depth);

	float screenPointX = static_cast<float>(x * width) / cScreenWidth;
	float screenPointY = static_cast<float>(y * height) / cScreenHeight+30;

	return D2D1::Point2F(screenPointX, screenPointY);
}

void ImageRenderer::ProcessSkeleton(NUI_SKELETON_FRAME skeletonFrame)
{

	int width = m_sourceWidth; //imageRect.right;
	int height = m_sourceHeight; //imageRect.bottom;

	for (int i = 0; i < NUI_SKELETON_COUNT; ++i)
	{
		NUI_SKELETON_TRACKING_STATE trackingState = skeletonFrame.SkeletonData[i].eTrackingState;

		if (NUI_SKELETON_TRACKED == trackingState)
		{
			// We're tracking the skeleton, draw it
			DrawSkeleton(skeletonFrame.SkeletonData[i], width, height);

		}
		else if (NUI_SKELETON_POSITION_ONLY == trackingState)
		{
			// we've only received the center point of the skeleton, draw that
			D2D1_ELLIPSE ellipse = D2D1::Ellipse(
				SkeletonToScreen(skeletonFrame.SkeletonData[i].Position, width, height),
				g_JointThickness,
				g_JointThickness
			);

			m_pRenderTarget->DrawEllipse(ellipse, m_pBrushJointTracked);
		}
	}

}

