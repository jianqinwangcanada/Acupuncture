using System;
using Microsoft.Extensions.Options;

namespace Acupuncture.CommonFunction.WritebleAppSettingFunction
{
    public interface IWritebleSettingSvc<out T>: IOptionsSnapshot<T> where T : class, new()
    {
        bool Update(Action<T> applyChange);
    }
}
