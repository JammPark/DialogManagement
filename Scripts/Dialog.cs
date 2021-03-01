using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JaeminPark.DialogManagement
{
    public class DialogLoadException : System.Exception
    {
        public DialogLoadException(string s, params string[] p) : base(string.Format(s, p)) { }
    }

    [CreateAssetMenu(menuName = "Dialog")]
    public class Dialog : ScriptableObject
    {
        public Subdialog dialog = new Subdialog();

        public IEnumerator Run(DialogManagerBase manager)
        {
            yield return dialog.Run(manager);
        }

        public void Load(DialogManagerBase manager)
        {
            dialog.Load(manager);
        }
    }

    [System.Serializable]
    public class Subdialog
    {
        [SerializeReference]
        public List<ActionBase> actions = new List<ActionBase>();

        public virtual IEnumerator Run(DialogManagerBase manager)
        {
            foreach (ActionBase action in actions)
                yield return action.Run(manager);
        }

        public virtual void Load(DialogManagerBase manager)
        {
            foreach (ActionBase action in actions)
            {
                action.Load(manager);
            }
        }
    }

    [System.Serializable]
    public class ParallelDialog : Subdialog
    {
        public override IEnumerator Run(DialogManagerBase manager)
        {
            foreach (ActionBase action in actions)
                yield return action.Run(manager);
        }
    }

    [System.Serializable]
    public abstract class ActionBase
    {
        public abstract void Load(DialogManagerBase manager);
        public abstract IEnumerator Run(DialogManagerBase manager);

        public static void Assert(bool condition, string text, params string[] param)
        {
            if (!condition)
                throw new DialogLoadException(text, param);
        }

        public static void AssertNotNull(object o, string text, params string[] param)
        {
            Assert(o != null, text, param);
        }
    }
}