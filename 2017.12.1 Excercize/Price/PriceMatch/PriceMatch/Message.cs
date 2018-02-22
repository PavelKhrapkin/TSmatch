/* ---------------------------------------------------------------------
 * Bootstrap - initialisation major global classes on application start
 * 
 * 15.02.2018 Pavel Khrapkin
 * 
 * --- History: ---
 *  5.02.18 transfer and adapted from Excercize/ReadPriceList
 *  7.02.18 use Properties.Setting.setting for getHpath()
 *  9.02.18 Boot.Init() in the main no-paramenter computer
 * 10.02.18 check TOCdir bug fix
 * 11.02.18 ExcelDir global string add
 * 14.02.18 public List<SuppliersInit> add to be loaded from SuppliersInit.xml
 * 15.02.18 move function getPriceLists into SuppliersInit to read AllSupliers
 * --- Methods: ---
 * Boot()   - Initial setting of global fields
 - getHpath()   - setup Hpath Dictionary taken from Properties,
                  check if TOCdir and ExcelDir are correct
 */
namespace PriceMatch
{
    public class Message
    {
    }
}