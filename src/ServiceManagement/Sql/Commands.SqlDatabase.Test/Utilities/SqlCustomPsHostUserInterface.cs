using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Test.Utilities
{
    public class SqlCustomPsHostUserInterface : PSHostUserInterface
    {
        /// <summary>
        /// An array of the inputs to supply to the prompt (in order)
        /// </summary>
        public PSObject[] PromptInputs { get; set; }

        /// <summary>
        /// The index of the option to choose when the user is prompted to choose from a list of options.
        /// </summary>
        public int PromptForChoiceInputIndex { get; set; }

        public SqlCustomPsHostUserInterface()
        {
            PromptForChoiceInputIndex = -1;
        }

        public override Dictionary<string, System.Management.Automation.PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            Assert.IsNotNull(PromptInputs);
            Assert.AreEqual(PromptInputs.Length, descriptions.Count, "The number of Prompt inputs needs to be the same as the number of prompts");
            
            Dictionary<string, PSObject> ret = new Dictionary<string, PSObject>();
            for (int i = 0; i < descriptions.Count; i++)
            {
                ret.Add(descriptions[i].Name, PromptInputs[i]);
            }
            return ret;
        }

        public override int PromptForChoice(string caption, string message, System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
        {
            Assert.IsTrue(PromptForChoiceInputIndex < choices.Count, "Must provide an index within the range of choices.");
            Assert.IsTrue(PromptForChoiceInputIndex >= 0, "Cannot have a negative index");

            return PromptForChoiceInputIndex;
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName, System.Management.Automation.PSCredentialTypes allowedCredentialTypes, System.Management.Automation.PSCredentialUIOptions options)
        {
            return null;
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return null;
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return null; }
        }

        public override string ReadLine()
        {
            return Console.ReadLine();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            var fg = Console.ForegroundColor;
            var bg = Console.BackgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(value);
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
        }

        public override void Write(string value)
        {
            Console.Write(value);
        }

        public override void WriteDebugLine(string message)
        {
            DebugRecord r = new DebugRecord(message);
            Console.WriteLine(r);
        }

        public override void WriteErrorLine(string value)
        {
            Console.Error.Write(value);
        }

        public override void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public override void WriteProgress(long sourceId, System.Management.Automation.ProgressRecord record)
        {
            Console.WriteLine("Source ID: " + sourceId);
            Console.WriteLine(record.CurrentOperation);
            Console.WriteLine(record.PercentComplete);
        }

        public override void WriteVerboseLine(string message)
        {
            Console.WriteLine("VERBOSE: " + message);
        }

        public override void WriteWarningLine(string message)
        {
            Console.WriteLine("WARNING: " + message);
        }
    }
}
