/*-----------------------------------------------------------------------
 * Interface -- Interface for CAD system Adapters - now Tekla and IFC.
 * 
 * 22.8.2016  Pavel Khrapkin
 *  
 *------ ToDo ----------
 * (!) реализовать Scale и SetScale в Tekla
 * (!) реализовать Scale и SetScaleв IFC
 * (!) реализовать init в Tekla без static.
 * (!) реализовать init c Document
 * (!) реализовать init c Module
 *----- History ------------------------------------------
 * 19.8.2016 started
 * -------------------------------------------
 */
using System.Collections.Generic;

namespace TSmatch
{
    interface IBase
    {
        void init(string arg);
    }
    interface IAdapterCAD : IBase         //Interface for CAD system Adapters - now Tekla and IFC.
    {
        List<ElmAttSet.ElmAttSet> Read();
        string getModelDir();
        string getModelName();
        string getModelMD5();
    }


} // end namespace
