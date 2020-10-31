
namespace Randgen
{
    public interface IGenerator<T>
    {
        T[] Take(int count);
        T TakeOne();
    }

}