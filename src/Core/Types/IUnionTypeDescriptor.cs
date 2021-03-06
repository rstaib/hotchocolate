namespace HotChocolate.Types
{
    public interface IUnionTypeDescriptor
    {
        IUnionTypeDescriptor Name(string name);
        IUnionTypeDescriptor Description(string description);
        IUnionTypeDescriptor Type<TObjectType>()
            where TObjectType : ObjectType;
        IUnionTypeDescriptor ResolveAbstractType(
            ResolveAbstractType resolveAbstractType);
    }
}
