using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public delegate void RefAction<T1>(ref T1 t1);
    public delegate void RefAction<T1,T2>(ref T1 t1,ref T2 t2);
    public delegate void RefAction<T1,T2,T3>(ref T1 t1,ref T2 t2,ref T3 t3);
    public delegate void RefAction<T1,T2,T3,T4>(ref T1 t1,ref T2 t2,ref T3 t3,ref T4 t4);

    public delegate TResult RefFunc<T1, TResult>(ref T1 t1) ;
    public delegate TResult RefFunc<T1,T2, TResult>(ref T1 t1,ref T2 t2);
    public delegate TResult RefFunc<T1,T2,T3, TResult>(ref T1 t1,ref T2 t2,ref T3 t3);
    public delegate TResult RefFunc<T1, T2, T3,T4, TResult>(ref T1 t1,ref T2 t2,ref T3 t3,ref T4 t4);
}
