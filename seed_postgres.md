### PostgreSQL: актуальный сидинг данных для EduTrack

Этот скрипт соответствует текущей модели данных и DbSet-ам в `EduTrackContext`:
`Users`, `Courses`, `Modules`, `Lessons`, `Assignments`, `AssignmentSubmissions`, `CourseEnrollments`, `LessonProgresses`, `Reviews`, `CourseTeachers`, `Tests`, `TestQuestions`, `TestAnswerOptions`, `TestSubmissions`, `TestSubmissionAnswers`, `Notifications`, `Announcements`, `DiscussionThreads`, `DiscussionMessages`, `Badges`, `UserBadges`, `CourseCertificates`, `WebinarSessions`, `NotificationPreferences`.

Что делает скрипт:
- Очищает все таблицы и сбрасывает идентификаторы (RESTART IDENTITY CASCADE)
- Генерирует детерминированные данные, совместимые со связями и уникальными ограничениями
- Не требует расширений PostgreSQL (работает в psql/pgAdmin)

Важные примечания:
- Перечисления хранятся как числа: `UserRole` (Administrator=0, Teacher=1, Student=2), `CourseCategory` (IT=0..Other=5), `DifficultyLevel` (Beginner=0..Expert=2), `CourseFormat` (Video=0..Mixed=3), `LessonType` (Video=0..Webinar=4), `NotificationType` (Deadline=0..System=3), `NotificationStatus` (Pending=0..Read=2).
- Пароли не хэшируются (см. `AuthService`). Вставляем в `PasswordHash` обычные строки паролей (например, `admin123`, `teacherN_pass`, `studentN_pass`).

Как применять (рекомендовано):
1) Убедитесь, что схема БД создана миграциями EF Core
   - Package Manager Console:
     - `Update-Database`
   - Или dotnet-CLI:
     - `dotnet ef database update`
2) Подготовьте БД/пользователя (при необходимости):
   - `CREATE DATABASE practika2;`
   - `CREATE USER postgres WITH PASSWORD '***';` (или используйте существующего пользователя)
   - Выдайте права пользователю на БД, если требуется.
3) Выполните скрипт:
   - В pgAdmin: откройте Query Tool и выполните целиком.
   - Через psql:
     - `psql "host=localhost port=5432 dbname=practika2 user=postgres password=***" -f seed.sql`

Советы по повторном запуске:
- Скрипт начинается с TRUNCATE ... RESTART IDENTITY CASCADE — его можно безопасно выполнять многократно.
- Если вы меняли схему через миграции, снова выполните `Update-Database`, а затем этот скрипт.

```sql
BEGIN;

-- 1) Очистка (без требований к порядку благодаря CASCADE)
TRUNCATE TABLE 
  "Notifications",
  "DiscussionMessages",
  "DiscussionThreads",
  "Announcements",
  "TestSubmissionAnswers",
  "TestSubmissions",
  "TestAnswerOptions",
  "TestQuestions",
  "Tests",
  "UserBadges",
  "Badges",
  "CourseCertificates",
  "WebinarSessions",
  "LessonProgresses",
  "AssignmentSubmissions",
  "Assignments",
  "Lessons",
  "Modules",
  "CourseTeachers",
  "Reviews",
  "CourseEnrollments",
  "Courses",
  "Users"
RESTART IDENTITY CASCADE;

-- 2) Пользователи: 1 админ, 40 преподавателей, 400 студентов
-- Administrator (1 шт)
INSERT INTO "Users" ("Username","Email","PasswordHash","FirstName","LastName","Role","IsBlocked","CreatedAt")
VALUES ('admin','admin@edutrack.local','admin123','Администратор','Системы',0,false, NOW());

-- Teachers (id начнутся с 2)
INSERT INTO "Users" ("Username","Email","PasswordHash","FirstName","LastName","Role","IsBlocked","CreatedAt")
SELECT 
  CONCAT('teacher', gs) AS "Username",
  CONCAT('teacher', gs, '@edutrack.local') AS "Email",
  CONCAT('teacher', gs, '_pass') AS "PasswordHash",
  CONCAT('Преподаватель', gs) AS "FirstName",
  CONCAT('Преп', gs) AS "LastName",
  1 AS "Role",
  false AS "IsBlocked",
  NOW() - (gs || ' days')::interval AS "CreatedAt"
FROM generate_series(1, 40) AS gs;

-- Students
INSERT INTO "Users" ("Username","Email","PasswordHash","FirstName","LastName","Role","IsBlocked","CreatedAt")
SELECT 
  CONCAT('student', gs) AS "Username",
  CONCAT('student', gs, '@edutrack.local') AS "Email",
  CONCAT('student', gs, '_pass') AS "PasswordHash",
  CONCAT('Студент', gs) AS "FirstName",
  CONCAT('Группа', (gs % 20) + 1) AS "LastName",
  2 AS "Role",
  false AS "IsBlocked",
  NOW() - ( (gs % 120) || ' days')::interval AS "CreatedAt"
FROM generate_series(1, 400) AS gs;

-- 3) Курсы: 30 курсов, создатели — админ или преподаватели по модулю
INSERT INTO "Courses" (
  "Title","Description","Category","Difficulty","Duration","Price","Format",
  "CoverImagePath","Rating","ReviewCount","IsPublished","IsArchived",
  "CreatedAt","UpdatedAt","CreatedById"
) 
SELECT 
  CONCAT('Курс #', gs) AS "Title",
  CONCAT('Подробное описание курса №', gs) AS "Description",
  ((gs - 1) % 6) AS "Category",
  ((gs - 1) % 3) AS "Difficulty",
  (20 + (gs % 10) * 5) AS "Duration",
  CASE WHEN (gs % 4) = 0 THEN 0 ELSE (1000 + (gs % 10) * 500) END AS "Price",
  ((gs - 1) % 4) AS "Format",
  NULL AS "CoverImagePath",
  0.00 AS "Rating",
  0 AS "ReviewCount",
  CASE WHEN (gs % 5) = 0 THEN false ELSE true END AS "IsPublished",
  false AS "IsArchived",
  NOW() - ( (gs % 60) || ' days')::interval AS "CreatedAt",
  NULL AS "UpdatedAt",
  CASE WHEN (gs % 3)=0 THEN 1 ELSE (2 + ((gs - 1) % 40)) END AS "CreatedById" -- 1=admin, иначе teacher
FROM generate_series(1, 30) AS gs;

-- 4) Привязка преподавателей к курсам: по 2 преподавателя на курс, детерминированно
WITH courses AS (SELECT "Id" AS course_id FROM "Courses"),
teachers AS (SELECT "Id" AS teacher_id FROM "Users" WHERE "Role" = 1 ORDER BY "Id")
INSERT INTO "CourseTeachers" ("CourseId","TeacherId")
SELECT c.course_id, (2 + ((c.course_id - 1 + off) % 40)) AS teacher_id
FROM courses c
JOIN (VALUES (0),(1)) AS offs(off) ON true
ON CONFLICT DO NOTHING;

-- 5) Модули: по 4 модуля на курс
WITH courses AS (SELECT "Id" AS course_id FROM "Courses")
INSERT INTO "Modules" ("Title","Description","Order","CourseId")
SELECT 
  CONCAT('Модуль ', m_idx, ' курса ', c.course_id) AS "Title",
  CONCAT('Описание модуля ', m_idx) AS "Description",
  m_idx AS "Order",
  c.course_id AS "CourseId"
FROM courses c
JOIN LATERAL generate_series(1, 4) AS m_idx ON true;

-- 6) Уроки: по 5 уроков на модуль
WITH mods AS (SELECT "Id","CourseId" FROM "Modules")
INSERT INTO "Lessons" ("Title","Description","Type","ContentUrl","Content","Duration","Order","ModuleId")
SELECT 
  CONCAT('Урок ', l_idx, ' модуля ', m."Id") AS "Title",
  CONCAT('Тематический урок №', l_idx) AS "Description",
  ((l_idx - 1) % 5) AS "Type",
  CASE WHEN (l_idx % 2) = 1 THEN CONCAT('https://cdn.example.com/video/', m."Id", '_', l_idx, '.mp4') ELSE NULL END AS "ContentUrl",
  CASE WHEN (l_idx % 2) = 0 THEN CONCAT('Текстовый контент урока ', l_idx) ELSE NULL END AS "Content",
  (10 + l_idx * 5) AS "Duration",
  l_idx AS "Order",
  m."Id" AS "ModuleId"
FROM mods m
JOIN LATERAL generate_series(1, 5) AS l_idx ON true;

-- 7) Задания: по 1 заданию на урок
WITH lessons AS (SELECT "Id" FROM "Lessons")
INSERT INTO "Assignments" ("Title","Description","MaxPoints","Deadline","LessonId")
SELECT 
  CONCAT('Задание урока ', l."Id", ' №1') AS "Title",
  'Описание задания' AS "Description",
  100 AS "MaxPoints",
  NOW() + INTERVAL '14 days' AS "Deadline",
  l."Id" AS "LessonId"
FROM lessons l;

-- 8) Записи на курсы: 20 студентов на курс (детерминированно)
WITH courses AS (SELECT "Id" AS course_id FROM "Courses")
INSERT INTO "CourseEnrollments" ("EnrolledAt","CompletedAt","Progress","CourseId","StudentId")
SELECT 
  NOW() - ((c.course_id % 30) || ' days')::interval AS "EnrolledAt",
  NULL AS "CompletedAt",
  (((rn - 1) * 5) % 101) AS "Progress",
  c.course_id AS "CourseId",
  (( (c.course_id - 1) * 20 + rn ) % 400) + (1 + 1 + 40) AS "StudentId" -- сдвиг после admin(1) и 40 преподавателей(2..41)
FROM (
  SELECT course_id FROM courses
) c
JOIN LATERAL (
  SELECT generate_series(1,20) AS rn
) g ON true
ON CONFLICT DO NOTHING;

-- 9) Прогресс по урокам: для каждой записи — по 3 первых урока курса
WITH enroll AS (
  SELECT e."Id", e."CourseId" FROM "CourseEnrollments" e
),
course_lessons AS (
  SELECT c."Id" AS course_id, l."Id" AS lesson_id
  FROM "Courses" c
  JOIN "Modules" m ON m."CourseId" = c."Id"
  JOIN "Lessons" l ON l."ModuleId" = m."Id"
)
INSERT INTO "LessonProgresses" ("IsCompleted","CompletedAt","ProgressPercentage","LessonId","EnrollmentId")
SELECT 
  CASE WHEN seq <= 2 THEN true ELSE false END AS "IsCompleted",
  CASE WHEN seq <= 2 THEN NOW() - INTERVAL '7 days' ELSE NULL END AS "CompletedAt",
  CASE WHEN seq = 1 THEN 100 WHEN seq = 2 THEN 100 ELSE 50 END AS "ProgressPercentage",
  cl.lesson_id AS "LessonId",
  e."Id" AS "EnrollmentId"
FROM enroll e
JOIN (
  SELECT course_id, lesson_id, ROW_NUMBER() OVER (PARTITION BY course_id ORDER BY lesson_id) AS seq
  FROM course_lessons
) cl ON cl.course_id = e."CourseId" AND cl.seq <= 3;

-- 10) Отзывы: по 10 отзывов на курс от разных студентов
WITH courses AS (SELECT "Id" AS course_id FROM "Courses"),
students AS (SELECT "Id" AS student_id FROM "Users" WHERE "Role" = 2)
INSERT INTO "Reviews" ("Rating","Comment","CreatedAt","CourseId","UserId")
SELECT 
  (1 + ((rn - 1) % 5)) AS "Rating",
  CONCAT('Отзыв #', rn, ' к курсу ', c.course_id) AS "Comment",
  NOW() - ((rn % 30) || ' days')::interval AS "CreatedAt",
  c.course_id AS "CourseId",
  (( (c.course_id - 1) * 10 + rn ) % 400) + (1 + 1 + 40) AS "UserId"
FROM courses c
JOIN LATERAL generate_series(1,10) AS rn ON true
ON CONFLICT DO NOTHING;

-- 11) Немного скорректируем количество отзывов и рейтинг в таблице "Courses"
UPDATE "Courses" c
SET "ReviewCount" = sub.rc,
    "Rating" = round(sub.avg_rating::numeric, 2)
FROM (
  SELECT "CourseId", COUNT(*) AS rc, AVG("Rating") AS avg_rating
  FROM "Reviews"
  GROUP BY "CourseId"
) AS sub
WHERE c."Id" = sub."CourseId";

-- 12) Тесты/викторины: по одному тесту к первым 2 урокам каждого модуля
WITH lessons AS (
  SELECT l."Id" AS lesson_id
  FROM "Lessons" l
  JOIN (
    SELECT "ModuleId","Id",
           ROW_NUMBER() OVER (PARTITION BY "ModuleId" ORDER BY "Id") AS rn
    FROM "Lessons"
  ) ranked ON ranked."Id" = l."Id"
  WHERE ranked.rn <= 2
)
INSERT INTO "Tests" ("Title","Description","TimeLimitMinutes","PassingScorePercent","LessonId")
SELECT 
  CONCAT('Тест к уроку ', lesson_id) AS "Title",
  'Проверьте свои знания' AS "Description",
  20 AS "TimeLimitMinutes",
  60 AS "PassingScorePercent",
  lesson_id AS "LessonId"
FROM lessons;

-- Вопросы (по 3 на тест), опции (4 на вопрос, 1 правильная)
WITH tests AS (SELECT "Id" AS test_id FROM "Tests")
INSERT INTO "TestQuestions" ("Text","Type","Order","TestId")
SELECT 
  CONCAT('Вопрос ', q, ' теста ', t.test_id) AS "Text",
  0 AS "Type",
  q AS "Order",
  t.test_id AS "TestId"
FROM tests t
JOIN LATERAL generate_series(1,3) AS q ON true;

WITH qs AS (SELECT "Id","TestId","Order" FROM "TestQuestions")
INSERT INTO "TestAnswerOptions" ("Text","IsCorrect","QuestionId")
SELECT 
  CONCAT('Вариант ', opt, ' для вопроса ', q."Id") AS "Text",
  CASE WHEN opt = 2 THEN true ELSE false END AS "IsCorrect",
  q."Id" AS "QuestionId"
FROM qs q
JOIN LATERAL generate_series(1,4) AS opt ON true;

-- 13) Объявления: по 2 анонса на курс от автора курса
WITH c AS (SELECT "Id","CreatedById" FROM "Courses")
INSERT INTO "Announcements" ("CourseId","AuthorId","Title","Content","CreatedAt")
SELECT 
  c."Id",
  c."CreatedById",
  CONCAT('Анонс #', rn, ' для курса ', c."Id"),
  'Новый материал и активности на неделе',
  NOW() - (rn || ' days')::interval
FROM c
JOIN LATERAL generate_series(1,2) AS rn ON true;

-- 14) Дискуссии: по 1 ветке на курс, по 3 сообщения
WITH courses AS (SELECT "Id" AS course_id FROM "Courses"),
first_student AS (SELECT MIN("Id") AS student_id FROM "Users" WHERE "Role"=2)
INSERT INTO "DiscussionThreads" ("CourseId","Title","CreatedAt","AuthorId")
SELECT c.course_id, CONCAT('Обсуждение курса ', c.course_id), NOW(), (SELECT student_id FROM first_student)
FROM courses c;

WITH threads AS (SELECT "Id" AS thread_id FROM "DiscussionThreads"),
students AS (SELECT "Id" FROM "Users" WHERE "Role"=2 ORDER BY "Id" LIMIT 3)
INSERT INTO "DiscussionMessages" ("ThreadId","AuthorId","Content","CreatedAt")
SELECT 
  t.thread_id,
  s."Id",
  CONCAT('Сообщение от студента ', s."Id"),
  NOW()
FROM threads t
CROSS JOIN students s;

-- 15) Вебинары: по одному ближайшему вебинару к каждому уроку с типом Webinar
WITH webinar_lessons AS (SELECT "Id" AS lesson_id FROM "Lessons" WHERE "Type" = 4)
INSERT INTO "WebinarSessions" ("LessonId","ScheduledAt","Link","RecordingUrl")
SELECT 
  wl.lesson_id,
  NOW() + INTERVAL '3 days',
  CONCAT('https://meet.example.com/lesson-', wl.lesson_id),
  NULL
FROM webinar_lessons wl;

-- 16) Бейджи: базовые записи и выдача части студентам
INSERT INTO "Badges" ("Code","Title","Description","Icon")
VALUES 
  ('FIRST_SUBMISSION','Первая отправка','Вы отправили первое домашнее задание',NULL),
  ('COURSE_FINISHER','Финишер курса','Вы завершили курс',NULL)
ON CONFLICT DO NOTHING;

WITH some_students AS (SELECT "Id" FROM "Users" WHERE "Role"=2 AND "Id" % 50 = 0)
INSERT INTO "UserBadges" ("UserId","BadgeId","AwardedAt")
SELECT s."Id",(SELECT "Id" FROM "Badges" WHERE "Code"='FIRST_SUBMISSION'), NOW()
FROM some_students s
ON CONFLICT DO NOTHING;

-- 17) Уведомления: по одному уведомлению о дедлайне для первых 100 студентов
WITH first_assign AS (SELECT "Id","Title","Deadline" FROM "Assignments" ORDER BY "Id" LIMIT 1),
students AS (SELECT "Id" AS student_id FROM "Users" WHERE "Role"=2 ORDER BY "Id" LIMIT 100)
INSERT INTO "Notifications" ("UserId","Type","Title","Message","Status","CreatedAt","DueAt")
SELECT 
  s.student_id,
  0, -- Deadline
  'Скоро дедлайн',
  CONCAT('Задание: ', (SELECT "Title" FROM first_assign)),
  0, -- Pending
  NOW(),
  (SELECT "Deadline" FROM first_assign)
FROM students s;

-- 18) Сертификаты: выдать сертификаты тем, у кого Progress=100
INSERT INTO "CourseCertificates" ("CourseId","StudentId","IssuedAt","CertificateNumber","FilePath")
SELECT e."CourseId", e."StudentId", NOW(),
  CONCAT('EDU-', e."CourseId", '-', e."StudentId", '-', to_char(NOW(),'YYYYMMDDHH24MISS')),
  NULL
FROM "CourseEnrollments" e
WHERE e."Progress" = 100
ON CONFLICT DO NOTHING;

-- 19) Настройки уведомлений: создать записи по умолчанию для всех пользователей
INSERT INTO "NotificationPreferences" ("UserId","DeadlinesEnabled","NewMaterialsEnabled","GradingResultsEnabled")
SELECT u."Id", true, true, true
FROM "Users" u
ON CONFLICT DO NOTHING;

COMMIT;
```

### Справка и проверка совместимости
- Хэширование паролей отключено: пароли сравниваются и сохраняются как есть (в `PasswordHash`). Скрипт вставляет обычные строки паролей и совместим с `AuthService`.
- Имена таблиц и колонок соответствуют аннотациям и соглашениям EF Core (кавычки сохранены для регистрозависимых идентификаторов).
- Если при выполнении появятся ошибки внешних ключей — убедитесь, что схема создана актуальными миграциями и что вы запускаете весь блок `BEGIN; ... COMMIT;` целиком.

