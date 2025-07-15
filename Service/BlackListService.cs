namespace BlogManagementApp.Service
{


    public interface IBlackListService
    {
        bool ContainsBlackListedWord(string word);
    }
    public class BlackListService : IBlackListService
    {
        public readonly HashSet<string> _blackList;

        public BlackListService(IEnumerable<String> blacklistwords)
        {
            _blackList = new HashSet<string>(blacklistwords.Select(x => x.ToLower().ToLower()), StringComparer.OrdinalIgnoreCase);
        }
        public bool ContainsBlackListedWord(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return false;

            var words = text.Split(new[] { ' ', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Any(w => _blackList.Contains(w.ToLower()));
        }
    }
}
