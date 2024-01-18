using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// CompilationPipeline.compilationStarted will call
    /// 
    /// EditorApplicationTools control call flow
    /// 
    /// method declare:
    /// 
    /// static void OnCompileStart(object obj){}
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CompileStartedAttribute : Attribute { }

    /// <summary>
    /// CompilationPipeline.compilationFinished will call
    /// 
    /// EditorApplicationTools control call flow
    /// method declare:
    /// 
    /// static void OnCompileStart(object obj){}
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CompileFinishedAttribute : Attribute { }
}
