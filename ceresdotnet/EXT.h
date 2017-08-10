using namespace Calibratie;
using namespace System::Runtime::CompilerServices;
using namespace System;


namespace ceresdotnet {
	[Extension]
	public ref class EXT abstract sealed
	{
	public:
		[Extension]
		static IntPtr test(PinholeCamera^ camera, IntPtr^ c){
			return IntPtr::Zero;
		}
	};
}

