﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

[assembly:ExtensionApplication(typeof(Autodesk.DesignScript.Geometry.DSApplication))]

namespace Autodesk.DesignScript.Geometry
{
    [Browsable(false)]
    public class DSApplication : IExtensionApplication
    {
        private static DSApplication mInstance = null;
        private List<IDisposable> mToBeDisposedObjects = new List<IDisposable>();
        private IExecutionSession mSession = null;
        private DSApplication()
        {
            mInstance = this;
        }

        public IExecutionSession Session
        {
            get
            {
                return mSession;
            }
        }
        
        public static DSApplication Instance
        {
            get
            {
                if (null == mInstance)
                    mInstance = new DSApplication();
                return mInstance;
            }
        }

        public bool IsExecuting { get { return mSession != null; } }

        void ToBeDisposed(IDisposable obj)
        {
            if (null == obj)
                return;

            lock (mToBeDisposedObjects)
            {
                mToBeDisposedObjects.Add(obj);
            }
        }

        void IExtensionApplication.OnBeginExecution(IExecutionSession session)
        {
            mSession = session;

            //Do not create HostFactory at this time
            if (null != HostFactory.CurrentInstance && !(HostFactory.ExtensionApplicationStarted))
            {
                foreach (IExtensionApplication app in HostFactory.ExtensionApplications)
                {
                    if (null != app)
                    {
                        app.OnBeginExecution(session);
                    }
                }
                HostFactory.ExtensionApplicationStarted = true;
            }

            DSGeometrySettings.Reset();
            DSGeometrySettings.RootModulePath = session.Configuration.RootModulePath;
            object temp = session.Configuration.GetConfigValue(ConfigurationKeys.GeometryXmlProperties);
            if (null != temp)
                DSGeometrySettings.GeometryXmlProperties = (bool)temp;
        }

        void IExtensionApplication.OnEndExecution(IExecutionSession session)
        {
            if (null != HostFactory.CurrentInstance && HostFactory.ExtensionApplicationStarted)
            {
                foreach (IExtensionApplication app in HostFactory.ExtensionApplications)
                {
                    if (null != app)
                    {
                        app.OnEndExecution(session);
                    }
                }
                HostFactory.ExtensionApplicationStarted = false;
            }

            if (null != mSession && !Object.ReferenceEquals(mSession, session))
                throw new System.InvalidOperationException("Session mismatch for begin and end execution call.");

            IDisposable[] disposables = null;
            lock (mToBeDisposedObjects)
            {
                disposables = mToBeDisposedObjects.ToArray();
                mToBeDisposedObjects.Clear();
            }

            foreach (var item in disposables)
            {
                item.Dispose();
            }
            
            mSession = null;
        }


        public void OnSuspendExecution(IExecutionSession session)
        {
            //TODO: Run GC
        }

        public void OnResumeExecution(IExecutionSession session)
        {
            //Reset the flag.
        }

        public void StartUp()
        {
            
        }

        public void ShutDown()
        {
            
        }
    }
}
