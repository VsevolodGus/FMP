using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace Bioss.Ultrasound.Services.Extensions
{
    internal static class CellExtension
    {
        /// <summary>
        /// Заполнение данными ячйки в таблице ПДФ отчета
        /// </summary>
        /// <param name="cell">ячейка</param>
        /// <param name="value">данные записываемые в ячейку</param>
        /// <param name="alignment">выравнивание</param>
        /// <returns></returns>
        public static Cell FillCell(this Cell cell,
            string value = null,
            ParagraphAlignment alignment = ParagraphAlignment.Left)
        {
            cell.AddParagraph(value ?? string.Empty);
            cell.Format.Alignment = alignment;

            return cell;
        }

        /// <summary>
        /// Заполнеие ячейки зеленой галочкой или красным крестиком
        /// </summary>
        /// <param name="cell">ячейка</param>
        /// <param name="resultCondition">условие выставление крестика или галочки</param>
        public static void FillBoolCell(this Cell cell, bool resultCondition)
        {
            (var color, var text) = TextForCellCriteriaBoudsman(resultCondition);
            cell.AddParagraph(text);
            cell.Format.Font.Color = color;

            static (Color, string) TextForCellCriteriaBoudsman(bool IsCriteia)
                => IsCriteia
                ? (Colors.Green, PdfOrderConstants.Yes)
                : (Colors.Red, PdfOrderConstants.No);
        }
    }
}
