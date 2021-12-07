#pragma once

#pragma managed(push, off) 
#include "ceres\loss_function.h"
#pragma managed(pop)

using namespace System;

namespace ceresdotnet {

#define DECLARE_LOSSFUNCTIONWRAPPER(lossf) public ref class lossf : public LossFunction
#define EVALUATE_WRAPPER virtual void Evaluate(double sq_norm, double out[3]) override { \
_lossfunction->Evaluate(sq_norm, out);\
}
#define LOSSF_CONSTRUCTOR(lossf,cereslossf) lossf(){ \
_lossfunction = new cereslossf(); \
	}
#define LOSSF_CONSTRUCTOR_D(lossf,cereslossf) \
	double _a;\
	lossf(double a){ \
_lossfunction = new cereslossf(a); \
 _a = a;\
	}
#define LOSSF_CONSTRUCTOR_D2(lossf,cereslossf) \
	double _a,_b;\
	lossf(double a, double b){ \
	_a = a;_b = b;\
_lossfunction = new cereslossf(a,b); \
	}

#define TOSTRINGOVERRIDE(str)String^ ToString() override {return str;}

	public ref class LossFunction abstract {


	internal:
		ceres::LossFunction* _lossfunction;

	public:
		!LossFunction() {
			if (_lossfunction != nullptr) {
				delete _lossfunction;
				_lossfunction = nullptr;
			}
		}
		~LossFunction() {
			this->!LossFunction();
		}
		
		virtual void Evaluate(double sq_norm, double out[3]) abstract;
	};

	DECLARE_LOSSFUNCTIONWRAPPER(TrivialLoss){
	public:
		LOSSF_CONSTRUCTOR(TrivialLoss, ceres::TrivialLoss);
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE("TrivialLoss");
	};

	DECLARE_LOSSFUNCTIONWRAPPER(HuberLoss)
	{
	public:
		LOSSF_CONSTRUCTOR_D(HuberLoss, ceres::HuberLoss);
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE("HuberLoss - a = " + _a);
	};
	

	DECLARE_LOSSFUNCTIONWRAPPER(SoftLOneLoss)
	{
	public:
		LOSSF_CONSTRUCTOR_D(SoftLOneLoss, ceres::SoftLOneLoss);
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE("SoftLOneLoss - a = " + _a);
	};

	DECLARE_LOSSFUNCTIONWRAPPER(CauchyLoss)
	{
	public:
		LOSSF_CONSTRUCTOR_D(CauchyLoss, ceres::CauchyLoss);
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE("CauchyLoss - a = " + _a);
	};

	DECLARE_LOSSFUNCTIONWRAPPER(ArctanLoss)
	{
	public:
		LOSSF_CONSTRUCTOR_D(ArctanLoss, ceres::ArctanLoss);
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE("ArctanLoss - a = " + _a);
	};

	DECLARE_LOSSFUNCTIONWRAPPER(TolerantLoss)
	{
	public:
		LOSSF_CONSTRUCTOR_D2(TolerantLoss, ceres::TolerantLoss);
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE("TolerantLoss - a = " + _a+" - b = "+_b);
	};

	DECLARE_LOSSFUNCTIONWRAPPER(TukeyLoss)
	{
	public:
		LOSSF_CONSTRUCTOR_D(TukeyLoss, ceres::TukeyLoss);
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE("TukeyLoss - a = " + _a);
	};

	DECLARE_LOSSFUNCTIONWRAPPER(ComposedLoss)
	{
	public:
		LossFunction^ loss1;
		LossFunction^ loss2;
		ComposedLoss(LossFunction^ a, LossFunction^ b)
		{
			loss1 = a;
			loss2 = b;
			_lossfunction = new ceres::ComposedLoss(a->_lossfunction, ceres::DO_NOT_TAKE_OWNERSHIP, b->_lossfunction, ceres::DO_NOT_TAKE_OWNERSHIP);
		}
		EVALUATE_WRAPPER;
		TOSTRINGOVERRIDE(loss1->ToString() + loss2->ToString());
	};

	DECLARE_LOSSFUNCTIONWRAPPER(ScaledLoss)
	{
	public:
		LossFunction^ _rho;
		double _a;
		ScaledLoss(LossFunction^ rho, double a)
		{
			this->_rho = rho;
			_a = a;
			_lossfunction = new ceres::ScaledLoss(rho->_lossfunction, a, ceres::DO_NOT_TAKE_OWNERSHIP);
		}
		TOSTRINGOVERRIDE("ScaledLoss(" + _a + ") - " + _rho->ToString());
	};

	DECLARE_LOSSFUNCTIONWRAPPER(LossFunctionWrapper)
	{
	public:
		LossFunction^ _rho;
		LossFunctionWrapper(LossFunction^ rho)
		{
			_rho = rho;
			_lossfunction = new ceres::LossFunctionWrapper(rho->_lossfunction, ceres::DO_NOT_TAKE_OWNERSHIP);
		}
		EVALUATE_WRAPPER;
		void Reset(LossFunction^ rho){
			_rho = rho;
			((ceres::LossFunctionWrapper*)_lossfunction)->Reset(rho->_lossfunction, ceres::DO_NOT_TAKE_OWNERSHIP);
		}

		TOSTRINGOVERRIDE(_rho->ToString());
	};
}