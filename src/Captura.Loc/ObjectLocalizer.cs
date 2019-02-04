namespace Captura.Loc
{
    public class ObjectLocalizer<T> : TextLocalizer
    {
        public ObjectLocalizer(T source, string localizationKey) : base(localizationKey)
        {
            Source = source;            
        }
        
        public T Source { get; }
    }
}
