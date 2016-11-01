//------------------------------------------------------------------------------
// <copyright file="SkeletonBasics.h" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "NuiApi.h"
#include "ImageRenderer.h"
#include "SpotMeBrain.h"

#define MAX_STRING_INPUT 100

class CSpotMeMain
{
    static const int        cScreenWidth  = 320;
    static const int        cScreenHeight = 240;

	static const int        cColorWidth = 640;
	static const int        cColorHeight = 480;

	static const int		cSecondViewWidth = 300;
	static const int		cSecondViewHeight = 300;


    static const int        cStatusMessageMaxLen = MAX_PATH*2;


public:
    /// <summary>
    /// Constructor
    /// </summary>
    CSpotMeMain();

    /// <summary>
    /// Destructor
    /// </summary>
    ~CSpotMeMain();

    /// <summary>
    /// Handles window messages, passes most to the class instance to handle
    /// </summary>
    /// <param name="hWnd">window message is for</param>
    /// <param name="uMsg">message</param>
    /// <param name="wParam">message data</param>
    /// <param name="lParam">additional message data</param>
    /// <returns>result of message processing</returns>
    static LRESULT CALLBACK MessageRouter(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);

    /// <summary>
    /// Handle windows messages for a class instance
    /// </summary>
    /// <param name="hWnd">window message is for</param>
    /// <param name="uMsg">message</param>
    /// <param name="wParam">message data</param>
    /// <param name="lParam">additional message data</param>
    /// <returns>result of message processing</returns>
    LRESULT CALLBACK        DlgProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);


    /// <summary>
    /// Creates the main window and begins processing
    /// </summary>
    /// <param name="hInstance"></param>
    /// <param name="nCmdShow"></param>
    int                     Run(HINSTANCE hInstance, int nCmdShow);


	bool StartCapture(int inputClassificationNumber, int numOfSamples, int captureDelay);

	CSpotMeBrain recognitionBrain;

	HWND                    m_hWnd;

private:
    

    bool                    m_bSeatedMode;

    // Current Kinect
    INuiSensor*             m_pNuiSensor;


    // Direct2D
    ID2D1Factory*           m_pD2DFactory;
	ImageRenderer*          m_pMainViewRender;
	ImageRenderer*			m_pSecondViewRender;
    
    HANDLE                  m_pSkeletonStreamHandle;
    HANDLE                  m_hNextSkeletonEvent;

	HANDLE                  m_pColorStreamHandle;
	HANDLE                  m_hNextColorFrameEvent;

	int						captureTimer;
	int						sampleCaptureCount;
	int						sampleCaptureDelay;
	int						sampleInputClassificationNum;
	bool					captureEnable;
	bool					sampleCaptureWait;
	bool					dragWindow;
	bool					setModeTContModeF; 
	int						originalWindowX;

	
	NUI_SKELETON_DATA lastSkeletonData;
	NUI_SKELETON_DATA lastStillSkeletonData;
	int lastPositionValue;

	HWND exerciseManagerDialoguePointer;
    
    /// <summary>
    /// Main processing function
    /// </summary>
    void                    Update();

    /// <summary>
    /// Create the first connected Kinect found 
    /// </summary>
    /// <returns>S_OK on success, otherwise failure code</returns>
    HRESULT                 CreateFirstConnected();

    /// <summary>
    /// Handle new skeleton data
    /// </summary>
    void                    ProcessSkeleton();

	//Combined
	void ProcessFrame();

	//Colour Data
	BYTE* ProcessColor();



	void HandleCaptureChecks();

    /// <summary>
    /// Set the status bar message
    /// </summary>
    /// <param name="szMessage">message to display</param>
    void                    SetStatusMessage(WCHAR* szMessage);


};
