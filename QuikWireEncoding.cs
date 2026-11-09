using System.Text;

namespace QuikSharp
{
    /// <summary>
    /// Кодировка сообщений между C# и Lua-скриптом в QUIK. QLua работает со строками в cp1251
    /// (названия бумаг, «Вид облигации», тексты ответов на транзакции), а dkjson передаёт байты строк
    /// как есть — поэтому и чтение, и запись идут в cp1251. Раньше транспорт читал UTF-8, и
    /// кириллица из QUIK превращалась в «�»; латиница и числа одинаковы в обеих кодировках.
    /// Символы вне cp1251 при записи заменяются на «?».
    /// </summary>
    public static class QuikWireEncoding
    {
        public static Encoding Cp1251 { get; }

        static QuikWireEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Cp1251 = Encoding.GetEncoding(1251);
        }
    }
}
