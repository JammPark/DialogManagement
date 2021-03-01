using System.Collections;
using UnityEngine;

namespace JaeminPark.DialogManagement
{
    public abstract class DialogManagerBase : MonoBehaviour
    {
        private Dialog dialog;
        public bool IsRunning { get; protected set; }

        public abstract void OnDialogStart();
        public abstract void OnDialogEnd();

        public void LoadDialog(Dialog dialogToLoad)
        {
            if (!IsRunning)
            {
                try
                {
                    dialogToLoad.Load(this);
                }
                catch (DialogLoadException e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public void StartDialog()
        {
            if (!IsRunning && dialog != null)
            {
                StartCoroutine(StartDialogCoroutine());
            }
        }

        private IEnumerator StartDialogCoroutine()
        {
            IsRunning = true;
            OnDialogStart();
            yield return dialog.Run(this);
            IsRunning = false;
            OnDialogEnd();
        }
    }
}