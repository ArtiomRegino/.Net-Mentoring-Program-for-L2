# Multi-Threading in .NET

## Part #1
- [X] 1.	Write a program, which creates an array of 100 Tasks, runs them and wait all of them are not finished. Each Task should iterate from 1 to 1000 and print into the console the following string: “Task #0 – {iteration number}”.
- [X] 2.	Write a program, which creates a chain of four Tasks. First Task – creates an array of 10 random integer. Second Task – multiplies this array with another random integer. Third Task – sorts this array by ascending. Fourth Task – calculates the average value. All this tasks should print the values to console
- [X] 3.	Write a program, which multiplies two matrices and uses class Parallel.
- [X] 4.	Write a program which recursively creates 10 threads. Each thread should be with the same body and receive a state with integer number, decrement it, print and pass as a state into the newly created thread. Use Thread class for this task and Join for waiting threads.
- [X] 5.	Write a program which recursively creates 10 threads. Each thread should be with the same body and receive a state with integer number, decrement it, print and pass as a state into the newly created thread. Use ThreadPool class for this task and Semaphore for waiting threads.
- [X] 6.	Write a program which creates two threads and a shared collection: the first one should add 10 elements into the collection and the second should print all elements in the collection after each adding. Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.
- [X] 7.	Create a Task and attach continuations to it according to the following criteria:
        a.	Continuation task should be executed regardless of the result of the parent task.
        b.	Continuation task should be executed when the parent task finished without success.
        c.	Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation.
        d.	Continuation task should be executed outside of the thread pool when the parent task would be cancelled.

Demonstrate the work of the each case with console utility.


## Part #2

- [X] 1. Напишите консольное приложение для асинхронного расчета суммы целых чисел от 0 до N. N задается пользователем из консоли. Пользователь вправе внести новую границу в процессе вычислений, что должно привести к перезапуску расчета. Это не должно привести к «падению» приложения.

- [X] 2. Напишите простейший менеджер закачек. Пользователь задает адрес страницы, которую необходимо загрузить. В процессе загрузки пользователь может ее отменить. Пользователь может задавать несколько источников для закачки. Скачивание страниц не должно блокировать интерфейс приложения.

- [X] 3. Напишите простейший магазин по заказу еды. Пользователь может выбрать товар, и он добавляется в корзину. При изменении товаров происходит автоматический пересчет стоимости. Любые действия пользователя с меню или корзиной не должны влиять на производительность UI (замораживать).

- [ ] 4. У вас есть Entity, которое описывает класс пользователя, хранящийся в БД. Пользователь хранит информацию об Имени, Фамилии, Возрасте. Напишите пример асинхронных CRUD операций для этого класса.


## Part #3
(В репозиории проект назван Chatter)

Общее

Наша задача – разработка клиента и сервера для простого корпоративного чата. Основные характеристики нашего решения:

* Сервер и клиент реализованы как обычные консольные или GUI приложения (на ваш выбор);

* Взаимодействие между клиентами и сервером осуществляется посредствам Named Pipes (System.IO.Pipes) или Sockets (System.Net.Sockets) – также на ваш выбор. Для простоты настройки можно хранить все параметры подключения в коде.

* Клиент представляет собой бот, который после запуска выполняет циклически:

    *  Подключается с новым именем к серверу;

    *  Отправляет несколько сообщений серверу (сообщения выбираются случайно из готового списка, количество отправляемых сообщений и паузы между ними также задаются случайно);

    *  Принимает все сообщения от сервера, которые выдают на экран и/или сохраняет в файл;

    *  Отключается от сервера.

Цикл повторяется до тех пор, пока пользователь не завершит работу клиента или не возникнет ошибка работы с сервером.

* Сервер:

    * Принимает подключение от клиента. При подключении узнает имя подключенного клиента.

    * Принимает от клиентов строки сообщений и рассылает их остальным подключенным клиентам

    * Хранит историю из N последних сообщений, которые рассылает клиентам при первом подключении

    * При завершении приложения рассылает клиентам уведомление и корректно закрывает все подключения

###Задание 1

- [ ] Реализуйте клиент и сервер используя для сервера схему «Для каждого клиента – свой поток обработки». Чтение и запись можно делать синхронными операциями.

###Задание 2

- [ ] Перепишите клиент и сервер используя (на выбор или совместно):

* Классические асинхронные операции (BeginXXX/EndXXX) и пул потоков для операций, которые вы инициируете сами.

* Task Parallel Library.