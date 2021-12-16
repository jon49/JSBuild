namespace JSBuild.Utils
{
    internal static class Hierarchy
    {
        public static List<List<FileData>> Get(FileData[] files)
        {
            var number = 1;
            var list = new List<List<FileData>>();
            var set = new HashSet<string>();

            // Seed
            list.Add(new List<FileData>());
            foreach (var x in files)
            {
                if (!x.Dependencies.Any())
                {
                    AddFile(x, 0);
                }
            }

            var count = 1;
            while (files.Length != set.Count && count != files.Length && count < 100)
            {
                list.Add(new List<FileData>());
                foreach (var x in files)
                {
                    if (x.InHierarchy) continue;
                    if (x.Dependencies.All(x => set.Contains(x.NormalizedName)))
                    {
                        AddFile(x, count);
                    }
                }
                count++;
            }

            if (count == 100)
            {
                foreach (var x in files)
                {
                    if (!x.InHierarchy)
                    {
                        Console.WriteLine(x.NormalizedName);
                    }
                }
            }
            return list;

            void AddFile(FileData file, int depth)
            {
                list[depth].Add(file);
                file.InHierarchy = true;
                set.Add(file.NormalizedName);
            }
        }
    }
}
