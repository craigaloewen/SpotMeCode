//------------------------------------------------------------------------------
// <copyright file="SkeletonBasics.cpp" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#include "stdafx.h"
#include <strsafe.h>
#include "SpotMeMain.h"
#include "resource.h"
#include <string>
#include "exerciseFileManager.h"



CSpotMeMain application;

/// <summary>
/// Entry point for the application
/// </summary>
/// <param name="hInstance">handle to the application instance</param>
/// <param name="hPrevInstance">always 0</param>
/// <param name="lpCmdLine">command line arguments</param>
/// <param name="nCmdShow">whether to display minimized, maximized, or normally</param>
/// <returns>status</returns>
int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow)
{
    application.Run(hInstance, nCmdShow);

}

/// <summary>
/// Constructor
/// </summary>
CSpotMeMain::CSpotMeMain() :
    m_pD2DFactory(NULL),
    m_hNextSkeletonEvent(INVALID_HANDLE_VALUE),
    m_pSkeletonStreamHandle(INVALID_HANDLE_VALUE),
    m_bSeatedMode(false),
    m_pNuiSensor(NULL),

	//Colour Additions
	m_pMainViewRender(NULL),
	m_pSecondViewRender(NULL),
	m_hNextColorFrameEvent(INVALID_HANDLE_VALUE),
	m_pColorStreamHandle(INVALID_HANDLE_VALUE),
	captureEnable(false),
	sampleCaptureWait(false),
	setModeTContModeF(false),
	dragWindow(false)
{
	captureTimer = 0;
	lastPositionValue = 0;
	sampleCaptureCount = 0;
	sampleCaptureDelay = 0;
	originalWindowX = 0;
}

/// <summary>
/// Destructor
/// </summary>
CSpotMeMain::~CSpotMeMain()
{
    if (m_pNuiSensor)
    {
        m_pNuiSensor->NuiShutdown();
    }

    if (m_hNextSkeletonEvent && (m_hNextSkeletonEvent != INVALID_HANDLE_VALUE))
    {
        CloseHandle(m_hNextSkeletonEvent);
    }

	if (m_hNextColorFrameEvent != INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_hNextColorFrameEvent);
	}


	delete m_pMainViewRender;
	m_pMainViewRender = NULL;

	delete m_pSecondViewRender;
	m_pSecondViewRender = NULL;

    // clean up Direct2D
    SafeRelease(m_pD2DFactory);

    SafeRelease(m_pNuiSensor);
}

//-------------------Dialogue Message Handlers

INT_PTR CALLBACK newExerciseDialogue(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG:
		return (INT_PTR)TRUE;
	case WM_COMMAND:
		if (LOWORD(wParam) == IDOK)
		{

			int len = GetWindowTextLength(GetDlgItem(hDlg, IDC_NEW_EXERCISE_INPUT));
			if (len > 0 && len < MAX_STRING_INPUT)
			{

				char inBuffer[MAX_STRING_INPUT];
				char outBuffer[MAX_STRING_INPUT];

				GetDlgItemText(hDlg, IDC_NEW_EXERCISE_INPUT, (LPWSTR)inBuffer, len + 1);

				SendMessage(GetDlgItem(GetParent(hDlg), IDC_EXERCISE_SELECT), CB_ADDSTRING, 0, (LPARAM)inBuffer);

				int i = SendMessage(GetDlgItem(GetParent(hDlg), IDC_EXERCISE_SELECT), CB_GETCOUNT, 0, 0);

				SendMessage(GetDlgItem(GetParent(hDlg), IDC_EXERCISE_SELECT), CB_SETCURSEL, i - 1, 0);



				int testMe = WideCharToMultiByte(CP_ACP, 0, (LPWSTR)inBuffer, MAX_STRING_INPUT, outBuffer, MAX_STRING_INPUT, NULL, NULL);

				std::string inputString(outBuffer);
				application.recognitionBrain.exerciseFileManager.addNewExercise(inputString);


				EndDialog(hDlg, LOWORD(wParam));
			}
			else {
				MessageBox(hDlg, L"Invalid Input", L"Error", 0);
			}
			return (INT_PTR)TRUE;
		}
		else if (LOWORD(wParam) == IDCANCEL) {

			EndDialog(hDlg, LOWORD(wParam));

			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;

}

INT_PTR CALLBACK statisticsDialogue(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG: {

		HFONT hFont = CreateFont(12, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, ANSI_CHARSET,
			OUT_TT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY,
			DEFAULT_PITCH | FF_DONTCARE, TEXT("MS Shell Dlg"));

		int labelYPlacement = 50;

		GRT::VectorFloat fMeasureResults = application.recognitionBrain.getFMeasures();

		for (int i = 0; i < application.recognitionBrain.exerciseFileManager.currentExercise.numOfClassifications; i++) {
			std::wstring tempString;


			tempString = std::wstring(application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].begin(), application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].end());
			tempString += L" - ";
			if (i < fMeasureResults.size()) {
				tempString += std::to_wstring(fMeasureResults[i]*100.0);
			}
			else {
				tempString += std::to_wstring(0);
			}

			const wchar_t* inputName = tempString.c_str();

			HWND hwndButton = CreateWindow(
				L"STATIC",  // Predefined class; Unicode assumed 
				inputName,      // Button text 
				WS_VISIBLE | WS_CHILD,  // Styles 
				10,         // x position 
				labelYPlacement,         // y position 
				200,        // Button width
				16,        // Button height
				hDlg,     // Parent window
				NULL,       // No menu.
				(HINSTANCE)GetWindowLong(hDlg, GWL_HINSTANCE),
				NULL);      // Pointer not needed.

			labelYPlacement += 15;

			SendMessage(hwndButton, WM_SETFONT, (WPARAM)hFont, TRUE);

		}

		return (INT_PTR)TRUE;
	}
	case WM_COMMAND:
		if (LOWORD(wParam) == IDCANCEL) {

			EndDialog(hDlg, LOWORD(wParam));

			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;

}


INT_PTR CALLBACK newClassifierDialogue(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG:
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_JOINT_SELECT), CB_ADDSTRING, 0, (LPARAM)L"None");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_JOINT_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Center Of Shoulders");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_JOINT_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Head");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_JOINT_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Shoulders");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_JOINT_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Elbows");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_JOINT_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Wrists");

		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_ARROW_SELECT), CB_ADDSTRING, 0, (LPARAM)L"None");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_ARROW_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Up");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_ARROW_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Right");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_ARROW_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Down");
		SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_ARROW_SELECT), CB_ADDSTRING, 0, (LPARAM)L"Left");

		return (INT_PTR)TRUE;
	case WM_COMMAND:
		if (LOWORD(wParam) == IDOK)
		{

			int lenName = GetWindowTextLength(GetDlgItem(hDlg, IDC_NEW_CLASSIFIER_NAME_INPUT));
			int lenMsg = GetWindowTextLength(GetDlgItem(hDlg, IDC_NEW_CLASSIFIER_MESSAGE_INPUT));
			if (lenName > 0 && lenName < MAX_STRING_INPUT && lenMsg > 0 && lenMsg < MAX_STRING_INPUT)
			{

				char inBuffer[MAX_STRING_INPUT];
				char outBuffer[MAX_STRING_INPUT];

				GetDlgItemText(hDlg, IDC_NEW_CLASSIFIER_NAME_INPUT, (LPWSTR)inBuffer, lenName + 1);

				SendMessage(GetDlgItem(GetParent(hDlg), IDC_CLASSIFIER_SELECT), CB_ADDSTRING, 0, (LPARAM)inBuffer);
				SendMessage(GetDlgItem(application.m_hWnd, IDC_SET_CLASS_SELECT), CB_ADDSTRING, 0, (LPARAM)inBuffer);

				int i = SendMessage(GetDlgItem(GetParent(hDlg), IDC_CLASSIFIER_SELECT), CB_GETCOUNT, 0, 0);

				SendMessage(GetDlgItem(GetParent(hDlg), IDC_CLASSIFIER_SELECT), CB_SETCURSEL, i - 1, 0);



				int testMe = WideCharToMultiByte(CP_ACP, 0, (LPWSTR)inBuffer, MAX_STRING_INPUT, outBuffer, MAX_STRING_INPUT, NULL, NULL);

				std::string nameInputString(outBuffer);

				GetDlgItemText(hDlg, IDC_NEW_CLASSIFIER_MESSAGE_INPUT, (LPWSTR)inBuffer, lenMsg + 1);

				testMe = WideCharToMultiByte(CP_ACP, 0, (LPWSTR)inBuffer, MAX_STRING_INPUT, outBuffer, MAX_STRING_INPUT, NULL, NULL);

				std::string messageInputString(outBuffer);

				int jointSelectValue = SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_JOINT_SELECT), CB_GETCURSEL, 0, 0);
				int arrowSelectValue = SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_INPUT_ARROW_SELECT), CB_GETCURSEL, 0, 0);

				if (jointSelectValue > 0) {
					jointSelectValue += 1;
				}
				else {
					jointSelectValue = 0;
				}

				if (arrowSelectValue == -1) {
					arrowSelectValue = 0;
				}

				application.recognitionBrain.exerciseFileManager.addClassification(nameInputString, messageInputString, jointSelectValue, arrowSelectValue);


				EndDialog(hDlg, LOWORD(wParam));
			}
			else {
				MessageBox(hDlg, L"Invalid Input", L"Error", 0);
			}
			return (INT_PTR)TRUE;
		}
		else if (LOWORD(wParam) == IDCANCEL) {

			EndDialog(hDlg, LOWORD(wParam));

			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;
}

BOOL CALLBACK exerciseManagerDialogueProc(HWND hDlg, UINT Message, WPARAM wParam, LPARAM lParam) {


	switch (Message)
	{
	case WM_INITDIALOG: {
		std::wstring inputString = std::to_wstring(application.recognitionBrain.getClassSampleNumber(0));
		SendDlgItemMessage(hDlg, IDC_NUMBER_OF_CLASSIFIER_SAMPLES, WM_SETTEXT, 0, (LPARAM)inputString.c_str());
		break;
	}
	case WM_COMMAND:
		switch (LOWORD(wParam))
		{

		case IDCANCEL:
			ShowWindow(hDlg, SW_HIDE);
			break;

		case IDC_EXERCISE_SELECT: {

			int hiWordID = HIWORD(wParam);

			switch (hiWordID) {

			case CBN_SELCHANGE: {

				int len = GetWindowTextLength(GetDlgItem(hDlg, IDC_EXERCISE_SELECT));
				int value = SendMessage(GetDlgItem(hDlg, IDC_EXERCISE_SELECT), CB_GETCURSEL, 0, 0);

				char inBuffer[MAX_STRING_INPUT];
				char outBuffer[MAX_STRING_INPUT];
				bool result;

				SendDlgItemMessageW(application.m_hWnd, IDC_INSTRUC_TEXT, WM_SETTEXT, 0, (LPARAM)L"Loading Exercise Data...");


				SendMessage(GetDlgItem(hDlg, IDC_EXERCISE_SELECT), CB_GETLBTEXT, value, (LPARAM)inBuffer);

				int testMe = WideCharToMultiByte(CP_ACP, 0, (LPWSTR)inBuffer, MAX_STRING_INPUT, outBuffer, MAX_STRING_INPUT, NULL, NULL);

				std::string inputString(outBuffer);

				result = application.recognitionBrain.exerciseFileManager.saveExercise();
				result = application.recognitionBrain.exerciseFileManager.loadExercise(inputString);

				SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_RESETCONTENT, (WPARAM)0, (LPARAM)0);
				for (int i = 0; i < application.recognitionBrain.exerciseFileManager.currentExercise.numOfClassifications; i++) {
					std::wstring tempString = std::wstring(application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].begin(), application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].end());
					const wchar_t* inputName = tempString.c_str();
					SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_ADDSTRING, 0, (LPARAM)inputName);
				}
				SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_SETCURSEL, (WPARAM)0, (LPARAM)0);

				SendMessage(GetDlgItem(application.m_hWnd, IDC_SET_CLASS_SELECT), CB_RESETCONTENT, (WPARAM)0, (LPARAM)0);
				for (int i = 0; i < application.recognitionBrain.exerciseFileManager.currentExercise.numOfClassifications; i++) {
					std::wstring tempString = std::wstring(application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].begin(), application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].end());
					const wchar_t* inputName = tempString.c_str();
					SendMessage(GetDlgItem(application.m_hWnd, IDC_SET_CLASS_SELECT), CB_ADDSTRING, 0, (LPARAM)inputName);
				}
				SendMessage(GetDlgItem(application.m_hWnd, IDC_SET_CLASS_SELECT), CB_SETCURSEL, (WPARAM)0, (LPARAM)0);


				if (!result) {
					MessageBox(hDlg, L"Something went wrong. Please restart.", L"Error", 0);
				}

				break;
			}
			default:
				break;

			}
			break;
		}
		case IDC_CLASSIFIER_SELECT: {

			int hiWordID = HIWORD(wParam);

			switch (hiWordID) {

				case CBN_SELCHANGE: {
					int classifierSelectValue = SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_GETCURSEL, 0, 0);
					classifierSelectValue += 1;

					std::wstring inputString = std::to_wstring(application.recognitionBrain.getClassSampleNumber(classifierSelectValue));
					SendDlgItemMessage(hDlg, IDC_NUMBER_OF_CLASSIFIER_SAMPLES, WM_SETTEXT, 0, (LPARAM)inputString.c_str());

					break;
				}
			}

			break;
		}
		case IDC_EXERCISE_NEW:
			DialogBox(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD_NEWEXERCISEBOX), hDlg, newExerciseDialogue);
			SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_RESETCONTENT, (WPARAM)0, (LPARAM)0);

			break;

		case IDC_CLASSIFIER_NEW: {
			DialogBox(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD_NEWCLASSIFIERBOX), hDlg, newClassifierDialogue);

			break;
		}

		case IDC_CLASSIFIER_DELETE: {

			int value = SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_GETCURSEL, 0, 0);

			if (value != -1) {
				application.recognitionBrain.exerciseFileManager.removeClassification(value);

				SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_DELETESTRING, (WPARAM)value, 0);
				SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_SETCURSEL, 0, 0);

				SendMessage(GetDlgItem(application.m_hWnd, IDC_SET_CLASS_SELECT), CB_DELETESTRING, (WPARAM)value, 0);
				SendMessage(GetDlgItem(application.m_hWnd, IDC_SET_CLASS_SELECT), CB_SETCURSEL, 0, 0);

			}



			break;
		}

		case IDC_EXERCISE_DELETE: {

			int value = SendMessage(GetDlgItem(hDlg, IDC_EXERCISE_SELECT), CB_GETCURSEL, 0, 0);


			if (value != -1) {

				application.recognitionBrain.exerciseFileManager.deleteCurrentExercise();

				SendMessage(GetDlgItem(hDlg, IDC_EXERCISE_SELECT), CB_DELETESTRING, (WPARAM)value, 0);
				SendMessage(GetDlgItem(hDlg, IDC_EXERCISE_SELECT), CB_SETCURSEL, 0, 0);

				SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_RESETCONTENT, (WPARAM)0, (LPARAM)0);
				for (int i = 0; i < application.recognitionBrain.exerciseFileManager.currentExercise.numOfClassifications; i++) {
					std::wstring tempString = std::wstring(application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].begin(), application.recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].end());
					const wchar_t* inputName = tempString.c_str();
					SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_ADDSTRING, 0, (LPARAM)inputName);
				}
				SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_SETCURSEL, (WPARAM)0, (LPARAM)0);
			}
			break;
		}
		case IDC_STARTCAPTURE: {

			application.StartCapture(SendMessage(GetDlgItem(hDlg, IDC_CLASSIFIER_SELECT), CB_GETCURSEL, 0, 0)+1,GetDlgItemInt(hDlg,IDC_CLASSIFICATION_SAMPLES,NULL,0), GetDlgItemInt(hDlg, IDC_CLASSIFICATION_CAPTURE_DELAY, NULL, 0));
			break;
		}
		}
		break;
	default:
		return FALSE;
	}
	return TRUE;


}

//-------------------Dialoge Message Handlers End


bool CSpotMeMain::StartCapture(int inputClassificationNumber, int numOfSamples, int captureDelay) {
	captureTimer = 0;
	m_pSecondViewRender->displayRecording = TRUE;
	sampleInputClassificationNum = inputClassificationNumber;
	sampleCaptureDelay = captureDelay;

	if (!sampleCaptureWait && !captureEnable) {
		sampleCaptureWait = TRUE;
		captureEnable = FALSE;
		sampleCaptureCount = numOfSamples;
	}
	else {
		sampleCaptureWait = FALSE;
		captureEnable = FALSE;
		sampleCaptureCount = 0;
	}
	return true;
}

/// <summary>
/// Creates the main window and begins processing
/// </summary>
/// <param name="hInstance">handle to the application instance</param>
/// <param name="nCmdShow">whether to display minimized, maximized, or normally</param>
int CSpotMeMain::Run(HINSTANCE hInstance, int nCmdShow)
{
    MSG       msg = {0};
    WNDCLASS  wc  = {0};

    // Dialog custom window class
    wc.style         = CS_HREDRAW | CS_VREDRAW;
    wc.cbWndExtra    = DLGWINDOWEXTRA;
    wc.hInstance     = hInstance;
    wc.hCursor       = LoadCursorW(NULL, IDC_ARROW);
    wc.hIcon         = LoadIconW(hInstance, MAKEINTRESOURCE(IDI_APP));
    wc.lpfnWndProc   = DefDlgProcW;
	wc.lpszMenuName = MAKEINTRESOURCE(IDC_SPOTMEMENU);
    wc.lpszClassName = L"SkeletonBasicsAppDlgWndClass";

    if (!RegisterClassW(&wc))
    {
        return 0;
    }

    // Create main application window
    HWND hWndApp = CreateDialogParamW(
        hInstance,
        MAKEINTRESOURCE(IDD_APP),
        NULL,
        (DLGPROC)CSpotMeMain::MessageRouter, 
        reinterpret_cast<LPARAM>(this));

    // Show window
    ShowWindow(hWndApp, nCmdShow);

    const int eventCount = 1;
    HANDLE hEvents[eventCount];

	

    // Main message loop
    while (WM_QUIT != msg.message)
    {
        hEvents[0] = m_hNextSkeletonEvent;

        // Check to see if we have either a message (by passing in QS_ALLEVENTS)
        // Or a Kinect event (hEvents)
        // Update() will check for Kinect events individually, in case more than one are signalled
        MsgWaitForMultipleObjects(eventCount, hEvents, FALSE, INFINITE, QS_ALLINPUT);

        // Explicitly check the Kinect frame event since MsgWaitForMultipleObjects
        // can return for other reasons even though it is signaled.
        Update();

        while (PeekMessageW(&msg, NULL, 0, 0, PM_REMOVE))
        {
            // If a dialog message will be taken care of by the dialog proc
            if ((hWndApp != NULL) && IsDialogMessageW(hWndApp, &msg))
            {
                continue;
            }

            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }
    }

    return static_cast<int>(msg.wParam);
}

void CSpotMeMain::HandleCaptureChecks() {



	if (sampleCaptureWait || captureEnable) {

		if (sampleCaptureWait) {
			if (captureTimer < 250) {
				m_pSecondViewRender->recordingDialogueValue = 250 - captureTimer;
			}
			else {
				sampleCaptureWait = FALSE;
				captureEnable = TRUE;
			}
		}

		if (captureEnable && captureTimer > sampleCaptureDelay) {
			
			if (sampleCaptureCount < 1) {
				captureEnable = FALSE;
				m_pSecondViewRender->displayRecording = FALSE;
				recognitionBrain.exerciseFileManager.saveExercise();
			}
			recognitionBrain.addSample(lastSkeletonData, sampleInputClassificationNum);

			m_pSecondViewRender->recordingDialogueValue = sampleCaptureCount;

			sampleCaptureCount--;
			captureTimer = 0;
		}
	}

}

/// <summary>
/// Main processing function
/// </summary>
void CSpotMeMain::Update()
{
    if (NULL == m_pNuiSensor)
    {
        return;
    }

	HandleCaptureChecks();

    // Wait for 0ms, just quickly test if it is time to process a skeleton
    if ( WAIT_OBJECT_0 == WaitForSingleObject(m_hNextSkeletonEvent, 0) || WAIT_OBJECT_0 == WaitForSingleObject(m_hNextColorFrameEvent, 0))
    {
        ProcessFrame();
		captureTimer++;
    }


}

/// <summary>
/// Handles window messages, passes most to the class instance to handle
/// </summary>
/// <param name="hWnd">window message is for</param>
/// <param name="uMsg">message</param>
/// <param name="wParam">message data</param>
/// <param name="lParam">additional message data</param>
/// <returns>result of message processing</returns>
LRESULT CALLBACK CSpotMeMain::MessageRouter(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    CSpotMeMain* pThis = NULL;

    if (WM_INITDIALOG == uMsg)
    {
        pThis = reinterpret_cast<CSpotMeMain*>(lParam);
        SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pThis));
    }
    else
    {
        pThis = reinterpret_cast<CSpotMeMain*>(::GetWindowLongPtr(hWnd, GWLP_USERDATA));
    }

    if (pThis)
    {
        return pThis->DlgProc(hWnd, uMsg, wParam, lParam);
    }

    return 0;
}

/// <summary>
/// Handle windows messages for the class instance
/// </summary>
/// <param name="hWnd">window message is for</param>
/// <param name="uMsg">message</param>
/// <param name="wParam">message data</param>
/// <param name="lParam">additional message data</param>
/// <returns>result of message processing</returns>
LRESULT CALLBACK CSpotMeMain::DlgProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_INITDIALOG:
        {
            // Bind application window handle
            m_hWnd = hWnd;

            // Init Direct2D
            D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &m_pD2DFactory);

			// Create and initialize a new Direct2D image renderer (take a look at ImageRenderer.h)
			// We'll use this to draw the data we receive from the Kinect to the screen
			m_pMainViewRender = new ImageRenderer();
			HRESULT hr = m_pMainViewRender->Initialize(GetDlgItem(m_hWnd, IDC_VIDEOVIEW), m_pD2DFactory, cColorWidth, cColorHeight, cColorWidth * sizeof(long), &recognitionBrain);
			if (FAILED(hr))
			{
				SetStatusMessage(L"Failed to initialize the Direct2D draw device.");
			}

			m_pSecondViewRender = new ImageRenderer();
			hr = m_pSecondViewRender->Initialize(GetDlgItem(m_hWnd, IDC_VIEWTWO), m_pD2DFactory, cSecondViewWidth, cSecondViewHeight, cSecondViewWidth * sizeof(long), &recognitionBrain);
			if (FAILED(hr))
			{
				MessageBox(hWnd, L"Didn't Get Resources", L"Warning!",
					MB_OK | MB_ICONINFORMATION);
			}


			//Set Font
			HFONT hFont = CreateFont(52, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, ANSI_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, DEFAULT_PITCH | FF_SWISS, L"MS Shell Dlg");
			hr = SendMessage(hWnd, WM_SETFONT, WPARAM(hFont), TRUE);

			HWND seatedItem = GetDlgItem(hWnd, IDC_INSTRUC_TEXT);

			SendMessage(seatedItem, WM_SETFONT, WPARAM(hFont), TRUE);

			SendDlgItemMessageW(hWnd, IDC_INSTRUC_TEXT, WM_SETTEXT, 0, (LPARAM)L"Raise Your Arms");

			//Create Brain
			recognitionBrain.initialize();


            // Look for a connected Kinect, and create it if found
            CreateFirstConnected();


			exerciseManagerDialoguePointer = CreateDialog(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD_EXERCISE_MANAGER), hWnd, exerciseManagerDialogueProc ,0);
			
			if (exerciseManagerDialoguePointer != NULL) {
				ShowWindow(exerciseManagerDialoguePointer, SW_SHOW);
			}
			else {
				MessageBox(hWnd, L"CreateDialog returned NULL", L"Warning!",
					MB_OK | MB_ICONINFORMATION);
			}

			recognitionBrain.exerciseFileManager.populateExercises();

			for (int i = 0; i < recognitionBrain.exerciseFileManager.exerciseNameList.size(); i++) {
				std::wstring tempString = std::wstring(recognitionBrain.exerciseFileManager.exerciseNameList[i].begin(), recognitionBrain.exerciseFileManager.exerciseNameList[i].end());
				const wchar_t* inputName = tempString.c_str();
				SendMessage(GetDlgItem(exerciseManagerDialoguePointer, IDC_EXERCISE_SELECT), CB_ADDSTRING, 0, (LPARAM)inputName);
			}
			SendMessage(GetDlgItem(exerciseManagerDialoguePointer, IDC_EXERCISE_SELECT), CB_SETCURSEL, (WPARAM)0, (LPARAM)0);


			for (int i = 0; i < recognitionBrain.exerciseFileManager.currentExercise.numOfClassifications; i++) {
				std::wstring tempString = std::wstring(recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].begin(), recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].end());
				const wchar_t* inputName = tempString.c_str();
				SendMessage(GetDlgItem(exerciseManagerDialoguePointer, IDC_CLASSIFIER_SELECT), CB_ADDSTRING, 0, (LPARAM)inputName);
			}
			SendMessage(GetDlgItem(exerciseManagerDialoguePointer, IDC_CLASSIFIER_SELECT), CB_SETCURSEL, (WPARAM)0, (LPARAM)0);

			ShowWindow(exerciseManagerDialoguePointer, SW_HIDE);

			for (int i = 0; i < recognitionBrain.exerciseFileManager.currentExercise.numOfClassifications; i++) {
				std::wstring tempString = std::wstring(recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].begin(), recognitionBrain.exerciseFileManager.currentExercise.classificationLabels[i].end());
				const wchar_t* inputName = tempString.c_str();
				SendMessage(GetDlgItem(hWnd, IDC_SET_CLASS_SELECT), CB_ADDSTRING, 0, (LPARAM)inputName);
			}
			SendMessage(GetDlgItem(hWnd, IDC_SET_CLASS_SELECT), CB_SETCURSEL, (WPARAM)0, (LPARAM)0);


			EnableWindow(GetDlgItem(hWnd, IDC_SET_MODE), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_FORWARDS), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_BACKWARDS), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_CONTINUOUS_MODE), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_RECORD), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_CLASS_SELECT), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_CLASS_SAVE), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_NUM_REPS), FALSE);

        }
        break;

	case WM_LBUTTONDOWN:
		dragWindow = true;
		originalWindowX = m_pSecondViewRender->skeletonAngle + (int)(short)LOWORD(lParam);
		SetCapture(hWnd);
		break;

	case WM_LBUTTONUP: {
		ReleaseCapture();
		m_pSecondViewRender->skeletonAngle = 0;
		dragWindow = false;
		break;
	}



	case WM_MOUSEMOVE: {
			if (dragWindow) {

				int xPosVal = (int)(short)LOWORD(lParam);

				m_pSecondViewRender->skeletonAngle = (int)(short)LOWORD(lParam) - originalWindowX;
			}
			break;
		}


        // If the titlebar X is clicked, destroy app
    case WM_CLOSE:
		recognitionBrain.closeBrain();
        DestroyWindow(hWnd);
        break;

    case WM_DESTROY:
        // Quit the main message pump
		recognitionBrain.closeBrain();
		PostQuitMessage(0);
        break;

        // Handle button press
    case WM_COMMAND:
        // If it was for the near mode control and a clicked event, change near mode

		switch (LOWORD(wParam)) {
		case IDC_SHOW_EXERCISE_MANAGER:
			ShowWindow(exerciseManagerDialoguePointer, SW_SHOW);
			break;
		case IDC_HIDE_EXERCISE_MANAGER:
			ShowWindow(exerciseManagerDialoguePointer, SW_HIDE);
			break;
		case IDC_SHOW_STATISTICS_BOX: {
			HWND testWnd = CreateDialog(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD_STATISTICSBOX), hWnd, statisticsDialogue, 0);
			ShowWindow(testWnd, SW_SHOW);
			break;
		}
		case IDC_CHECK_SEATED: {
			// Toggle out internal state for near mode
			m_bSeatedMode = !m_bSeatedMode;

			if (NULL != m_pNuiSensor)
			{
				// Set near mode for sensor based on our internal state
				m_pNuiSensor->NuiSkeletonTrackingEnable(m_hNextSkeletonEvent, m_bSeatedMode ? NUI_SKELETON_TRACKING_FLAG_ENABLE_SEATED_SUPPORT : 0);
			}
			break;
		}
		case IDC_EXERCISE_MANAGER_SHOW_OR_HIDE: {

			if (IsWindowVisible(exerciseManagerDialoguePointer)) {
				ShowWindow(exerciseManagerDialoguePointer, SW_HIDE);
				SetDlgItemText(hWnd, IDC_EXERCISE_MANAGER_SHOW_OR_HIDE, L"Open Exercise Manager");
			} else {
				ShowWindow(exerciseManagerDialoguePointer, SW_SHOW);
				SetDlgItemText(hWnd, IDC_EXERCISE_MANAGER_SHOW_OR_HIDE, L"Close Exercise Manager");
			}
				

			break;
		}
		case IDC_CONTINUOUS_MODE: {
			EnableWindow(GetDlgItem(hWnd, IDC_SET_MODE), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_FORWARDS), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_BACKWARDS), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_CONTINUOUS_MODE), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_RECORD), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_CLASS_SELECT), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_CLASS_SAVE), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_NUM_REPS), FALSE);
			setModeTContModeF = false;
			recognitionBrain.setMode = false;
			break;
		}
		case IDC_SET_MODE: {
			EnableWindow(GetDlgItem(hWnd, IDC_SET_MODE), FALSE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_FORWARDS), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_BACKWARDS), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_CONTINUOUS_MODE), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_RECORD), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_CLASS_SELECT), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_CLASS_SAVE), TRUE);
			EnableWindow(GetDlgItem(hWnd, IDC_SET_NUM_REPS), TRUE);
			setModeTContModeF = true;
			recognitionBrain.setMode = true;
			recognitionBrain.clearSkeletonList();
			break;
		}

		case IDC_SET_RECORD: {
			recognitionBrain.startRecording(GetDlgItemInt(hWnd, IDC_SET_NUM_REPS, NULL, FALSE));
			break;
		}

		case IDC_SET_BACKWARDS: {
			recognitionBrain.selectPreviousRep();
			break;
		}

		case IDC_SET_FORWARDS: {
			recognitionBrain.selectNextRep();
			break;
		}

		case IDC_SET_CLASS_SAVE: {
			int instructVal = SendMessage(GetDlgItem(hWnd, IDC_SET_CLASS_SELECT), CB_GETCURSEL, 0, 0) + 1;
			recognitionBrain.addSample(recognitionBrain.getCurrentRepSkeleton(),instructVal);
			break;
		}


		}
        break;
    }

    return FALSE;
}

/// <summary>
/// Create the first connected Kinect found 
/// </summary>
/// <returns>indicates success or failure</returns>
HRESULT CSpotMeMain::CreateFirstConnected()
{
    INuiSensor * pNuiSensor;

    int iSensorCount = 0;
    HRESULT hr = NuiGetSensorCount(&iSensorCount);
    if (FAILED(hr))
    {
        return hr;
    }

    // Look at each Kinect sensor
    for (int i = 0; i < iSensorCount; ++i)
    {
        // Create the sensor so we can check status, if we can't create it, move on to the next
        hr = NuiCreateSensorByIndex(i, &pNuiSensor);
        if (FAILED(hr))
        {
            continue;
        }

        // Get the status of the sensor, and if connected, then we can initialize it
        hr = pNuiSensor->NuiStatus();
        if (S_OK == hr)
        {
            m_pNuiSensor = pNuiSensor;
            break;
        }

        // This sensor wasn't OK, so release it since we're not using it
        pNuiSensor->Release();
    }

    if (NULL != m_pNuiSensor)
    {
        // Initialize the Kinect and specify that we'll be using skeleton
        hr = m_pNuiSensor->NuiInitialize(0x0000000A);

		if (FAILED(hr)) {
			MessageBox(NULL, L"Initialize Fail", L"Note", MB_OK);
		}

        if (SUCCEEDED(hr))
        {
            // Create an event that will be signaled when skeleton data is available
            m_hNextSkeletonEvent = CreateEventW(NULL, TRUE, FALSE, NULL);

            // Open a skeleton stream to receive skeleton data
            hr = m_pNuiSensor->NuiSkeletonTrackingEnable(m_hNextSkeletonEvent, 0); 

			if (FAILED(hr)) {
				MessageBox(NULL, L"Skeleton Fail", L"Note", MB_OK);
			}


			// Create an event that will be signaled when color data is available
			m_hNextColorFrameEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

			// Open a color image stream to receive color frames
			hr = m_pNuiSensor->NuiImageStreamOpen(
				NUI_IMAGE_TYPE_COLOR,
				NUI_IMAGE_RESOLUTION_640x480,
				0,
				2,
				m_hNextColorFrameEvent,
				&m_pColorStreamHandle);

			if (FAILED(hr)) {
				MessageBox(NULL, L"Image Stream Error", L"Note", MB_OK);
			}

        }
    }

    if (NULL == m_pNuiSensor || FAILED(hr))
    {
        SetStatusMessage(L"No ready Kinect found!");
        return E_FAIL;
    }

    return hr;
}


/// <summary>
/// Handle new color data
/// </summary>
/// <returns>indicates success or failure</returns>
void CSpotMeMain::ProcessFrame()
{
	HRESULT hr;
	NUI_IMAGE_FRAME imageFrame;

	


	NUI_SKELETON_FRAME skeletonFrame = { 0 };
	short flags = 1;

	hr = m_pNuiSensor->NuiSkeletonGetNextFrame(0, &skeletonFrame);
	if (FAILED(hr))
	{
		skeletonFrame = { 0 };
		flags = 0;
	}
	else {

		// smooth out the skeleton data
		m_pNuiSensor->NuiTransformSmooth(&skeletonFrame, NULL);

		int minDepth = 100;

		for (int i = 0; i < NUI_SKELETON_COUNT; i++) {
			if (skeletonFrame.SkeletonData[i].eTrackingState == NUI_SKELETON_TRACKED) {
				if (skeletonFrame.SkeletonData[i].Position.z < minDepth) {
					lastSkeletonData = skeletonFrame.SkeletonData[i];
					minDepth = skeletonFrame.SkeletonData[i].Position.z;
				}
				
			}
		}

		lastPositionValue = recognitionBrain.getPredictedClassLabel(lastSkeletonData);

		//TestCode

		std::string strInput = recognitionBrain.getClassificationString();

		int nInputLength = (int)strInput.length() + 1;
		int nLength = MultiByteToWideChar(CP_ACP, 0, strInput.c_str(), nInputLength, 0, 0);

		wchar_t* pbuffer = new wchar_t[nLength];
		MultiByteToWideChar(CP_ACP, 0, strInput.c_str(), nInputLength, pbuffer, nLength);

		std::wstring wstrReturn(pbuffer);

		delete[] pbuffer;
		pbuffer = NULL;

		SetDlgItemText(m_hWnd, IDC_INSTRUC_TEXT, wstrReturn.c_str());


	}

	// Attempt to get the color frame
	hr = m_pNuiSensor->NuiImageStreamGetNextFrame(m_pColorStreamHandle, 0, &imageFrame);
	if (FAILED(hr))
	{
		return;
	}

	INuiFrameTexture * pTexture = imageFrame.pFrameTexture;
	NUI_LOCKED_RECT LockedRect;

	// Lock the frame data so the Kinect knows not to modify it while we're reading it
	pTexture->LockRect(0, &LockedRect, NULL, 0);

	// Make sure we've received valid data
	if (LockedRect.Pitch != 0)
	{
		// Draw the data with Direct2D
		m_pMainViewRender->DrawFrame(static_cast<BYTE *>(LockedRect.pBits), LockedRect.size, lastSkeletonData, flags);

		if (setModeTContModeF) {
			m_pSecondViewRender->DrawFrameStaticSkeleton(recognitionBrain.getCurrentRepSkeleton(), lastPositionValue, recognitionBrain.lastClassValues);
		}
		else {
			m_pSecondViewRender->DrawFrameStaticSkeleton(lastSkeletonData, lastPositionValue, recognitionBrain.pipeline.getClassLikelihoods());
		}
		
	}

	// We're done with the texture so unlock it
	pTexture->UnlockRect(0);

	// Release the frame
	m_pNuiSensor->NuiImageStreamReleaseFrame(m_pColorStreamHandle, &imageFrame);

	//Update Set UI
	if (setModeTContModeF) {
		SetDlgItemInt(m_hWnd, IDC_SET_MAX_TEXT, recognitionBrain.getSetSize(), FALSE);
		SetDlgItemInt(m_hWnd, IDC_SET_CURRENT_TEXT, recognitionBrain.getSetPositionValue(), FALSE);
	}



}





/// <summary>
/// Set the status bar message
/// </summary>
/// <param name="szMessage">message to display</param>
void CSpotMeMain::SetStatusMessage(WCHAR * szMessage)
{
    SendDlgItemMessageW(m_hWnd, IDC_INSTRUC_TEXT, WM_SETTEXT, 0, (LPARAM)szMessage);
}