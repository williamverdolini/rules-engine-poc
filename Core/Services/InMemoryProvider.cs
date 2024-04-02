using System.Collections.Concurrent;

namespace Core.Services
{
    public class InMemoryProvider
    {
        private readonly ConcurrentDictionary<string, dynamic> _objects = new();

        public void UpsertInput(string inputKey, dynamic? jsonObj)
        {
            if (string.IsNullOrEmpty(inputKey))
            {
                throw new ArgumentNullException(nameof(inputKey));
            }
            if (jsonObj != null)
            {
                _objects[inputKey] = jsonObj;
            }
            else
            {
                throw new ArgumentNullException(nameof(jsonObj));
            }
        }

        public RuleInput? GetObjectInput(string inputKey) {
            if (!_objects.TryGetValue(inputKey, out var obj))
            {
                return null;
            }
            return new RuleInput(inputKey, obj);
        }
    }
}
