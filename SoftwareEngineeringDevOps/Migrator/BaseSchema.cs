using FluentMigrator;
using System.Runtime.CompilerServices;

namespace Migrator
{
    public abstract class BaseSchema : Migration
    {
        IEnumerable<string> Scripts { get; }
        long Version;
        public BaseSchema(IEnumerable<string> scripts)
        {
            MigrationAttribute myMigration = GetType().GetCustomAttributes(true).FirstOrDefault(a => a is MigrationAttribute) as MigrationAttribute 
                ?? throw new ApplicationException($"MigrationAttribute missing from {GetType().Name}");
            Version = myMigration.Version;
            Scripts = scripts;
        }

        void ExecuteScripts(IEnumerable<string> scripts, [CallerMemberName] string direction = null)
        {
            foreach (string script in scripts)
                Execute.EmbeddedScript($"{direction}V{Version}_{script}.sql");
        }

        public override void Up() => ExecuteScripts(Scripts);

        public override void Down() => ExecuteScripts(Scripts.Reverse());
    }
}
