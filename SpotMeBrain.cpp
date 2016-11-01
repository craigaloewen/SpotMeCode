
#include "SpotMeBrain.h"

CSpotMeBrain::CSpotMeBrain() {

	trainingData = new GRT::ClassificationData;
	derivativeValue = 0; //XXX
	exerciseFileManager.trainingData = trainingData;
	exerciseFileManager.pipelinePointer = &pipeline;
	skeletonListPosition = 0;
	repsLeftToCapture = 0;
	setMode = false;
	detectedSet = false;
	lastSetValue = 0;


}

CSpotMeBrain::~CSpotMeBrain() {

	delete(trainingData);


}

bool CSpotMeBrain::closeBrain() {

	return exerciseFileManager.saveExercise();
}

GRT::VectorFloat CSpotMeBrain::getFMeasures() {
	return pipeline.getTestFMeasure();
}

bool CSpotMeBrain::initialize() {

	//GRT::ANBC anbcModel;

	//anbcModel.setNullRejectionCoeff(10);
	//anbcModel.enableScaling(true);
	//anbcModel.enableNullRejection(true);

	//pipeline.setClassifier(GRT::ANBC());
	pipeline.setClassifier(GRT::SVM(GRT::LINEAR));
	
	numDimensions = 21; //XXX
	trainingData->setNumDimensions(numDimensions);

	derivativeModule.init(1, 0.1, numDimensions, true, 3);

	exerciseFileManager.populateExercises();

	return true;
}


int CSpotMeBrain::getClassSampleNumber(int classNumber) {
	int numClasses =  trainingData->getNumClasses();
	if (classNumber > numClasses || numClasses <= 0 || classNumber <= 0) {
		return 0;
	}

	GRT::Vector<unsigned int> testVector = trainingData->getNumSamplesPerClass();
	return testVector[classNumber-1];

}

bool CSpotMeBrain::addSample(NUI_SKELETON_DATA skeletonData, UINT classLabel) {

	if (skeletonData.eTrackingState != NUI_SKELETON_TRACKED) {
		return false;
	}


	GRT::VectorFloat inputVector = vectorizeSkeleton(skeletonData);

	if (inputVector.empty()) {
		return false;
	}
	
	trainingData->addSample(classLabel, inputVector);

	return true;

}

GRT::VectorFloat CSpotMeBrain::vectorizeTwoJoints(Vector4 joint1, Vector4 joint2, double yRotationAngle, double zRotationAngle) {

	Vector4 outputVector;
	Vector4 storeVector;

	double deltaX = (joint1.x - joint2.x);
	double deltaY = (joint1.y - joint2.y);
	double deltaZ = (joint1.z - joint2.z);

	double vectorLength = sqrt(deltaX*deltaX + deltaY*deltaY + deltaZ*deltaZ);

	outputVector.x = ((deltaX / vectorLength));
	outputVector.y = ((deltaY / vectorLength));
	outputVector.z = ((deltaZ / vectorLength)); 

	double radianYAngle = yRotationAngle;
	double radianZAngle = zRotationAngle;

	storeVector.x = cos(radianYAngle)*outputVector.x + sin(radianYAngle)*outputVector.z;
	storeVector.y = outputVector.y;
	storeVector.z = -sin(radianYAngle)*outputVector.x + cos(radianYAngle)*outputVector.z;


	outputVector.x = cos(radianZAngle)*storeVector.x - sin(radianZAngle) * storeVector.y;
	outputVector.y = sin(radianZAngle)*storeVector.x + cos(radianZAngle) * storeVector.y;
	outputVector.z = storeVector.z;

	GRT::VectorFloat floatOutputVector;

	floatOutputVector.push_back(outputVector.x);
	floatOutputVector.push_back(outputVector.y);
	floatOutputVector.push_back(outputVector.z);

	return floatOutputVector;

}



Vector4 CSpotMeBrain::vectorizeTwoJointsNoRotation(Vector4 joint1, Vector4 joint2) {

	Vector4 outputVector;

	double deltaX = (joint1.x - joint2.x);
	double deltaY = (joint1.y - joint2.y);
	double deltaZ = (joint1.z - joint2.z);

	double vectorLength = sqrt(deltaX*deltaX + deltaY*deltaY + deltaZ*deltaZ);

	outputVector.x = ((deltaX / vectorLength));
	outputVector.y = ((deltaY / vectorLength));
	outputVector.z = ((deltaZ / vectorLength));



	return outputVector;

}


GRT::VectorFloat CSpotMeBrain::vectorizeSkeleton(const NUI_SKELETON_DATA &skeletonData) {

	GRT::VectorFloat outputVector;
	GRT::VectorFloat jointVector;
	Vector4 angleVector;
	Vector4 inputAngleVector;

	for (int i = 0; i < 8; i++) {
		if (!(skeletonData.eSkeletonPositionTrackingState[NUI_SKELETON_POSITION_SHOULDER_CENTER + i] == (NUI_SKELETON_POSITION_TRACKED) || skeletonData.eSkeletonPositionTrackingState[NUI_SKELETON_POSITION_SHOULDER_CENTER + i] == (NUI_SKELETON_POSITION_INFERRED))) {
			return jointVector;
		}
	}

	
	int i;
	double yAngle, zAngle;

	Vector4 shoulderLineVector;
	Vector4 shoulderLineVectorStored;

	shoulderLineVector.x = skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].x - skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].x;
	shoulderLineVector.y = skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].y - skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].y;
	shoulderLineVector.z = skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT].z - skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT].z;

	yAngle = atan2(shoulderLineVector.z, shoulderLineVector.x);

	shoulderLineVectorStored = shoulderLineVector;

	shoulderLineVector.x = cos(yAngle)*shoulderLineVectorStored.x + sin(yAngle)*shoulderLineVectorStored.z;
	shoulderLineVector.y = shoulderLineVectorStored.y;
	shoulderLineVector.z = -sin(yAngle)*shoulderLineVectorStored.x + cos(yAngle)*shoulderLineVectorStored.z;

	zAngle = atan2(shoulderLineVector.y, shoulderLineVector.x);



	jointVector = vectorizeTwoJoints(skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_CENTER], skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_HEAD],yAngle,zAngle);
	outputVector.insert(outputVector.end(), jointVector.begin(), jointVector.end());



	for (int i = 1; i < 4; i++) {
		jointVector = vectorizeTwoJoints(skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT], skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_LEFT+i], yAngle, zAngle);
		outputVector.insert(outputVector.end(), jointVector.begin(), jointVector.end());
	}

	for (int i = 1; i < 4; i++) {
		jointVector = vectorizeTwoJoints(skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT], skeletonData.SkeletonPositions[NUI_SKELETON_POSITION_SHOULDER_RIGHT + i], yAngle, zAngle);
		outputVector.insert(outputVector.end(), jointVector.begin(), jointVector.end());
	}



	return outputVector;



}

double CSpotMeBrain::computeTotalDerivative(GRT::VectorFloat derivVector) {

	GRT::VectorFloat computeVector = derivativeModule.computeDerivative(derivVector);
	double sum = 0;

	for (int i = 0; i < computeVector.size(); i++) {
		sum += abs(computeVector[i]);
	}

	return sum;

}

std::string CSpotMeBrain::getClassificationString() {
	int inNum;

	if (setMode) {
		inNum = lastSetValue;

		if (inNum > exerciseFileManager.currentExercise.numOfClassifications || inNum <= 0) {
			return std::string("");
		}

		return exerciseFileManager.currentExercise.classificationMessages[lastSetValue - 1];

	}
	else {
		inNum = pipeline.getPredictedClassLabel() - 1;

		if (inNum >= exerciseFileManager.currentExercise.numOfClassifications || inNum < 0) {
			return std::string("");
		}

		return exerciseFileManager.currentExercise.classificationMessages[pipeline.getPredictedClassLabel() - 1];
	}


}

UINT CSpotMeBrain::getPredictedClassLabel(const NUI_SKELETON_DATA &skeletonData) {

	GRT::VectorFloat inputVector = vectorizeSkeleton(skeletonData);


	if (inputVector.empty()) {
		return 0;
	}
	
	derivativeValue = computeTotalDerivative(inputVector)*100;

	pipeline.predict(inputVector);

	// Check if Reps and add derivatives
	if (setMode) {
		if (repsLeftToCapture > 0) {
			if (derivativeValue < 20 && derivativeValue != 0) {
				if (!detectedSet) {
					addSkeletonSetData(skeletonData);
					lastSetValue = pipeline.getPredictedClassLabel();
					lastClassValues = pipeline.getClassLikelihoods();
					repsLeftToCapture--;
					detectedSet = true;
				}
			}
			else if (derivativeValue > 150) {
				detectedSet = false;
			}
		}
		return lastSetValue;
	}


	return pipeline.getPredictedClassLabel();



}

bool CSpotMeBrain::selectNextRep()
{
	if (getSetSize() == 0) {
		return false;
	}

	if (skeletonListPosition == (getSetSize()-1)) {
		return false;
	}
	
	skeletonListPosition++;
	GRT::VectorFloat inputVector = vectorizeSkeleton(skeletonList[skeletonListPosition]);
	pipeline.predict(inputVector);
	lastSetValue = pipeline.getPredictedClassLabel();
	lastClassValues = pipeline.getClassLikelihoods();


	return true;
}

bool CSpotMeBrain::selectPreviousRep()
{
	if (getSetSize() == 0) {
		return false;
	}

	if (skeletonListPosition == 0) {
		return false;
	}

	skeletonListPosition--;
	GRT::VectorFloat inputVector = vectorizeSkeleton(skeletonList[skeletonListPosition]);
	pipeline.predict(inputVector);
	lastSetValue = pipeline.getPredictedClassLabel();
	lastClassValues = pipeline.getClassLikelihoods();

	return true;
}

UINT CSpotMeBrain::getSetSize()
{
	return skeletonList.size();
}

NUI_SKELETON_DATA CSpotMeBrain::getCurrentRepSkeleton()
{
	if (getSetSize() == 0) {
		return NUI_SKELETON_DATA();
	}

	return skeletonList[skeletonListPosition];

}

bool CSpotMeBrain::clearSkeletonList()
{
	skeletonList.clear();
	skeletonListPosition = 0;
	return true;
}

bool CSpotMeBrain::addSkeletonSetData(NUI_SKELETON_DATA addSkeletonData)
{
	skeletonList.push_back(addSkeletonData);

	skeletonListPosition = skeletonList.size() - 1;
	GRT::VectorFloat inputVector = vectorizeSkeleton(skeletonList[skeletonListPosition]);
	pipeline.predict(inputVector);
	lastSetValue = pipeline.getPredictedClassLabel();
	lastClassValues = pipeline.getClassLikelihoods();


	return true;
}

bool CSpotMeBrain::startRecording(int numOfReps)
{
	if (!setMode) {
		return false;
	}

	if (numOfReps < 0) {
		return false;
	}

	clearSkeletonList();
	repsLeftToCapture = numOfReps;

	return true;
}

int CSpotMeBrain::getSetPositionValue()
{

	if (getSetSize() == 0) {
		return 0;
	}
	else {
		return skeletonListPosition + 1;
	}

}


