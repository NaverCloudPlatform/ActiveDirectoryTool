using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public static class IoC
    {
        // ninject
        public static IKernel Kernel { get; private set; } = new StandardKernel();
        public static IUIManager UI => IoC.Get<IUIManager>();
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }            

        public static void Setup()
        {
            BindViewModels();
        }

        private static void BindViewModels()
        {
            Kernel.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());
        }
    }
}
