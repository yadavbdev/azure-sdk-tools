using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Test.Utilities
{
    public class SqlTestPsHost : PSHost
    {
        private SqlCustomPsHostUserInterface ui;
        private Guid id = Guid.NewGuid();
        private string name = "SqlTestPsHost";
        private Version version = new Version(1, 0, 0, 0);

        public SqlTestPsHost()
        {
            ui = new SqlCustomPsHostUserInterface();
        }

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return Thread.CurrentThread.CurrentCulture; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override Guid InstanceId
        {
            get { return id; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override void NotifyBeginApplication()
        {
            return;
        }

        public override void NotifyEndApplication()
        {
            return;
        }

        public override void SetShouldExit(int exitCode)
        {
            return;
        }

        public override PSHostUserInterface UI
        {
            get { return ui; }
        }

        public override Version Version
        {
            get { return version; }
        }
    }
}
