using Newtonsoft.Json;
using RulesEngine.Models;

namespace Core.Services
{
    public class RulesEngineLocalJsonStore : IRulesEngineStore
    {
        private readonly string _path;
        private readonly FileSystemWatcher _watcher;
        private List<Workflow> _workflows;

        public RulesEngineLocalJsonStore()
        {
            _path = Path.Combine(Directory.GetCurrentDirectory(), @"..\Core\Data\sample_workflow.json");
            _watcher = new();
            _workflows = GetWorkflowsFromLocalJson(_path) ?? [];
            _watcher.Path = Path.GetDirectoryName(_path)!;
            _watcher.Filter = "*.*";
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += OnChanged;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            _workflows = GetWorkflowsFromLocalJson(_path) ?? [];
        }

        public Task<List<Workflow>> GetAllWorkflowsAsync()
        {
            return Task.FromResult(_workflows);
        }

        public Task<Workflow?> GetWorkflowByName(string workflowName)
        {
            return Task.FromResult(_workflows.FirstOrDefault(w => w.WorkflowName == workflowName));
        }

        public Task<List<Workflow>> GetWorkflowByNames(string[] workflowNames)
        {
            return Task.FromResult(
                _workflows
                    ?.Where(w => workflowNames.Contains(w.WorkflowName))
                    ?.ToList() ?? []);
        }

        private static List<Workflow>? GetWorkflowsFromLocalJson(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Rules not found.");
            }

            var fileData = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<Workflow>>(fileData);
        }
    }
}
