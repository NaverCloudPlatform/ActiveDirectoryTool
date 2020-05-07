using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdTool.Core
{
    public enum ElementHorizontalAlignment
    {
        //
        // 요약:
        //     요소는 부모 요소에 대 한 레이아웃 슬롯의 왼쪽에 맞춥니다.
        Left = 0,
        //
        // 요약:
        //     요소는 부모 요소에 대 한 레이아웃 슬롯의 가운데에 맞춥니다.
        Center = 1,
        //
        // 요약:
        //     요소는 부모 요소에 대 한 레이아웃 슬롯의 오른쪽에 맞춥니다.
        Right = 2,
        //
        // 요약:
        //     요소는 부모 요소의 전체 레이아웃 슬롯에 맞게 늘어납니다.
        Stretch = 3
    }
}
