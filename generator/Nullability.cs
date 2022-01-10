public static class Nullability
{
    public static bool IsNullableRequiredForType(string type)
    {
        return type == "int" || type == "bool";
    }
}
