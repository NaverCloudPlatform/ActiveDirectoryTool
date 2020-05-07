using AdTool.Core;

namespace AdTool
{
    public class ViewModelLocator
    {
        public static ViewModelLocator Instance { get; private set; } = new ViewModelLocator();
        public static ApplicationViewModel ApplicationViewModel => IoC.Get<ApplicationViewModel>();

    }
}
