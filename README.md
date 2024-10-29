    public static string GenNum(Random random)
    {
        List<int> nums = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        string number = "";
        for (int i = 0; i < 4; i++)
        {
            int index = random.Next(0, nums.Count);
            number += nums[index];
            nums.RemoveAt(index);
        }
        return number;
    }
