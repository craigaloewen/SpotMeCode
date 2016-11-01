#ifndef SPOTMEBRAIN_H
#define SPOTMEBRAIN_H

#include "GRT\GRT.h"
#include "stdlib.h"
#include "NuiApi.h"
#include "exerciseFileManager.h"
#include <cmath>
#include <vector>
#include <string>

class CSpotMeBrain {

public:

	CSpotMeBrain();
	~CSpotMeBrain();

	bool initialize();
	bool closeBrain();
	bool addSample(NUI_SKELETON_DATA skeletonData, UINT classLabel);

	int getClassSampleNumber(int classNumber);

	UINT getPredictedClassLabel(const NUI_SKELETON_DATA &skeletonData);

	bool selectNextRep();
	bool selectPreviousRep();
	UINT getSetSize();
	NUI_SKELETON_DATA getCurrentRepSkeleton();
	bool clearSkeletonList();
	bool addSkeletonSetData(NUI_SKELETON_DATA addSkeletonData);
	bool startRecording(int numOfReps);
	int getSetPositionValue();


	GRT::GestureRecognitionPipeline pipeline;

	double computeTotalDerivative(GRT::VectorFloat derivVector);

	double derivativeValue;
	CExerciseFileManager exerciseFileManager;

	std::string getClassificationString();

	GRT::VectorFloat getFMeasures();

	std::vector<NUI_SKELETON_DATA> skeletonList;
	int skeletonListPosition;
	int repsLeftToCapture;
	int lastSetValue;
	GRT::VectorFloat lastClassValues;
	bool setMode;
	bool detectedSet;

private:

	
	GRT::ClassificationData* trainingData;

	GRT::Derivative derivativeModule;

	UINT numDimensions;

	GRT::VectorFloat vectorizeSkeleton(const NUI_SKELETON_DATA &skeletonData);
	GRT::VectorFloat vectorizeTwoJoints(Vector4 joint1, Vector4 joint2, double yRotationAngle, double zRotationAngle);
	Vector4 vectorizeTwoJointsNoRotation(Vector4 joint1, Vector4 joint2);
	

	




	


};



#endif //SPOTMEBRAIN_H

