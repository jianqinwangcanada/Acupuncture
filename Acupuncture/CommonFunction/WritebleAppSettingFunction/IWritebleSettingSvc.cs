using System;
namespace Acupuncture.CommonFunction.WritebleAppSettingFunction
{
    public interface IWritebleSettingSvc<out T>
    {
        bool Update(Action<T> applyChange);
    }
}
