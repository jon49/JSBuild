namespace JSBuild.Utils
{
    internal static class Hierarchy
    {
        public static List<List<FileData>> Get(FileData[] files)
        {
            var list = new List<List<FileData>>();
            var set = new HashSet<string>();

            // Seed
            list.Add(new List<FileData>());
            foreach (var x in files)
            {
                if (!x.Dependencies.Any())
                {
                    list[0].Add(x);
                    x.InHierarchy = true;
                    set.Add(x.NormalizedName);
                }
            }

            var count = 1;
            while (files.Length != set.Count)
            {
                list.Add(new List<FileData>());
                foreach (var x in files)
                {
                    if (x.InHierarchy) continue;
                    if (x.Dependencies.All(x => set.Contains(x.NormalizedName)))
                    {
                        list[count].Add(x);
                        x.InHierarchy = true;
                        set.Add(x.NormalizedName);
                    }
                }
                count++;
            }
            return list;
        }
    }
}
