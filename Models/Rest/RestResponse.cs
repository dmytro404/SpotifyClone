namespace SpotifyClone.Models.Rest
{
    public class RestResponse
    {
        public RestStatus Status { get; set; } = new();
        public RestMeta Meta { get; set; } = new();
        public Object? Data { get; set; }
    }
}
/* REST (Representational State Transfer) - набір архітектурних вимог/принципів,
 * реалізація яких покращує роботу розподілених систем.
 * 
 * API (Application Program Interface) - інтерфейс взаємодії Програми з своїми
 * Застосунками
 * у цьому контексті Програма - інформаційний центр, зазвичай бекенд
 * Застосунок - самостійна частина, зазвичай клієнтського призначення,
 * яка взаємодіє з Програмою шляхом обміну даними.
 * (* також Додаток - несамостійна програма - плагін або розширення *)
 * 
 *                                      REST
 *                  Program  -----------API------------- Other
 *             API/    |API  \API
 *               /     |      \
 *         Mobile   Desktop    Site             
 *         
 * Принципи
 * Client/Server – вимога до відокремленості архітектурних частин
 * Stateless – кожен запит сприймається Програмою як новий, жодні стани не зберігаються (у т.ч. сесії)
 * Cache – у відповіді мають бути відомості про можливість кешування
 * Layered system – можливість додати до шляху запитів-відповідей проміжні вузли
 * Code on demand (optional) – можливість включення коду (HTML/JS, SVG, ...)
 * Uniform interface
 * - Resource identification in requests: всі дані про ресурс мають бути у запиті (маршрутизація)
 * - Resource manipulation through representations: відповідь повинна містити метадані з 
 *     відомостями про оновлення чи видалення ресурсу
 * - Self-descriptive messages: відповідь має описувати який тип даних вона містить
 * - Hypermedia as the engine of application state (HATEOAS): у відповіді мають бути дані
 *     про зміст (перелік) підлеглих ресурсів
 *     
 *     
 */
/* Приклад REST-відповіді
{                                 | Принцип Layered system легше реалізувати якщо 
    "status": {                   | розділити статуси НТТР та статуси оброблення.
        "isOK": true,             | - НТТР 404 - не знайдений сам ресурс
        "code": 200,              | - НТТР 200 + status=404 - ресурс знайдений, а даних немає
        "phrase": "Ok"            | 
    },                            | 
    "meta": {
        "manipulations": ["GET", "POST", "DELETE"],
        "service": "Shop API",
        "serverTime": 1234987651,
        "url": "https://shop.site/api",
        "cache": 84600,
        "opt": {
            "page": 2,
            "lastPage": 10,
            "perPage": 20,
            "total": 193
        },
        "links": {
            "cart": "https://shop.site/api/cart",
            "signIn": "https://shop.site/api/user",
        },
        "dataType": "json/object"
    },
    "data": {
        "groups": [...],
        "actions": [...],
        "topSale": [...]
    }
}

Д.З. Перевести роботу бекенд частини проєкту 
на архітектуру REST
 */