public static class ListExtension
{
    public static int GetContentHashCode<T>(this List<T> list)
    {
        int res = 0;
        foreach (T item in list)
        {
            res ^= item?.GetHashCode() ?? 0;
            res <<= 1; // 让哈希值与顺序相关
        }
        return res;
    }
}