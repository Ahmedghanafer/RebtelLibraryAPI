-- 01_Books.sql - SQL Seed Script for Books Table
-- Inserts 50+ diverse books across multiple categories with realistic metadata
-- Data designed to showcase analytics capabilities with popular and niche books

-- Popular Fiction Books (High borrow count potential)
INSERT INTO Books (Id, Title, Author, ISBN, PageCount, Category, Availability, CreatedAt, UpdatedAt)
VALUES ('11111111-1111-1111-1111-111111111111', 'The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', 180,
        'Fiction', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111112', '1984', 'George Orwell', '9780452284234', 328, 'Fiction', 0, GETDATE(),
        GETDATE()),
       ('11111111-1111-1111-1111-111111111113', 'To Kill a Mockingbird', 'Harper Lee', '9780061120084', 376, 'Fiction',
        0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111114', 'Pride and Prejudice', 'Jane Austen', '9780141439518', 432, 'Fiction',
        0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111115', 'The Catcher in the Rye', 'J.D. Salinger', '9780316769480', 277,
        'Fiction', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111116', 'The Lord of the Rings', 'J.R.R. Tolkien', '9780544003415', 1216,
        'Fantasy', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111117', 'Harry Potter and the Sorcerer''s Stone', 'J.K. Rowling',
        '9780439708188', 309, 'Fantasy', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111118', 'The Hunger Games', 'Suzanne Collins', '9780439023481', 374,
        'Science Fiction', 0, GETDATE(), GETDATE()),

-- Technology and Computer Science Books (Popular in tech libraries)
       ('11111111-1111-1111-1111-111111111119', 'Clean Code', 'Robert C. Martin', '9780132350884', 464, 'Technology', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111120', 'The Pragmatic Programmer', 'Andrew Hunt', '9780201616224', 352,
        'Technology', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111121', 'Design Patterns', 'Erich Gamma', '9780201633610', 395, 'Technology', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111122', 'Refactoring', 'Martin Fowler', '9780201485677', 448, 'Technology', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111123', 'Introduction to Algorithms', 'Thomas H. Cormen', '9780262033848', 1312,
        'Technology', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111124', 'The Art of Computer Programming', 'Donald Knuth', '9780201896848', 672,
        'Technology', 0, GETDATE(), GETDATE()),

-- Science Books
       ('11111111-1111-1111-1111-111111111125', 'A Brief History of Time', 'Stephen Hawking', '9780553380163', 256,
        'Science', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111126', 'The Selfish Gene', 'Richard Dawkins', '9780199291151', 384, 'Science',
        0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111127', 'Cosmos', 'Carl Sagan', '9780345531359', 432, 'Science', 0, GETDATE(),
        GETDATE()),
       ('11111111-1111-1111-1111-111111111128', 'The Origin of Species', 'Charles Darwin', '9780140432054', 504,
        'Science', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111129', 'Silent Spring', 'Rachel Carson', '9780618249060', 378, 'Science', 0,
        GETDATE(), GETDATE()),

-- Non-Fiction and Biography
       ('11111111-1111-1111-1111-111111111130', 'Steve Jobs', 'Walter Isaacson', '9781451648539', 656, 'Biography', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111131', 'The Diary of a Young Girl', 'Anne Frank', '9780553296983', 283,
        'Biography', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111132', 'Long Walk to Freedom', 'Nelson Mandela', '9780316548182', 630,
        'Biography', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111133', 'Educated', 'Tara Westover', '9780399590504', 334, 'Biography', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111134', 'Sapiens', 'Yuval Noah Harari', '9780062316097', 443, 'Non-Fiction', 0,
        GETDATE(), GETDATE()),

-- History Books
       ('11111111-1111-1111-1111-111111111135', 'Guns, Germs, and Steel', 'Jared Diamond', '9780393317558', 480,
        'History', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111136', 'The Rise and Fall of the Third Reich', 'William L. Shirer',
        '9781568496677', 1280, 'History', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111137', 'A People''s History of the United States', 'Howard Zinn',
        '9780060838652', 729, 'History', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111138', 'The Wright Brothers', 'David McCullough', '9781476728745', 336,
        'History', 0, GETDATE(), GETDATE()),

-- Mystery and Thriller
       ('11111111-1111-1111-1111-111111111139', 'The Girl with the Dragon Tattoo', 'Stieg Larsson', '9780307473479',
        672, 'Mystery', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111140', 'Gone Girl', 'Gillian Flynn', '9780307588371', 432, 'Mystery', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111141', 'The Da Vinci Code', 'Dan Brown', '9780385504205', 689, 'Mystery', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111142', 'And Then There Were None', 'Agatha Christie', '9780007119318', 264,
        'Mystery', 0, GETDATE(), GETDATE()),

-- Romance
       ('11111111-1111-1111-1111-111111111143', 'The Notebook', 'Nicholas Sparks', '9780446367146', 214, 'Romance', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111144', 'Me Before You', 'Jojo Moyes', '9780143126544', 369, 'Romance', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111145', 'P.S. I Love You', 'Cecelia Ahern', '9780786868825', 506, 'Romance', 0,
        GETDATE(), GETDATE()),

-- Children Books
       ('11111111-1111-1111-1111-111111111146', 'The Cat in the Hat', 'Dr. Seuss', '9780394800016', 61, 'Children', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111147', 'Where the Wild Things Are', 'Maurice Sendak', '9780060254926', 48,
        'Children', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111148', 'Charlotte''s Web', 'E.B. White', '9780061125859', 184, 'Children', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111149', 'The Very Hungry Caterpillar', 'Eric Carle', '9780399226908', 26,
        'Children', 0, GETDATE(), GETDATE()),

-- Reference and Textbook
       ('11111111-1111-1111-1111-111111111150', 'The Elements of Style', 'William Strunk Jr.', '9780205309023', 105,
        'Reference', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111151', 'Merriam-Webster''s Collegiate Dictionary', 'Merriam-Webster',
        '9780877791768', 1664, 'Reference', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111152', 'Calculus', 'James Stewart', '9780495559749', 1368, 'Textbook', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111153', 'Physics for Scientists and Engineers', 'Raymond A. Serway',
        '9781133954327', 1568, 'Textbook', 0, GETDATE(), GETDATE()),

-- Poetry and Drama
       ('11111111-1111-1111-1111-111111111154', 'The Complete Poems of Emily Dickinson', 'Emily Dickinson',
        '9780316184134', 704, 'Poetry', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111155', 'Leaves of Grass', 'Walt Whitman', '9780140421996', 624, 'Poetry', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111156', 'Hamlet', 'William Shakespeare', '9780743477128', 342, 'Drama', 0,
        GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111157', 'Death of a Salesman', 'Arthur Miller', '9780140247732', 144, 'Drama',
        0, GETDATE(), GETDATE()),

-- Additional niche books for recommendation testing
       ('11111111-1111-1111-1111-111111111158', 'Zen and the Art of Motorcycle Maintenance', 'Robert M. Pirsig',
        '9780060839871', 540, 'Non-Fiction', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111159', 'The Master and Margarita', 'Mikhail Bulgakov', '9780060883454', 400,
        'Fiction', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111160', 'Neuromancer', 'William Gibson', '9780441569569', 271,
        'Science Fiction', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111161', 'The Man Who Was Thursday', 'G.K. Chesterton', '9780142437989', 224,
        'Fiction', 0, GETDATE(), GETDATE()),
       ('11111111-1111-1111-1111-111111111162', 'Thinking, Fast and Slow', 'Daniel Kahneman', '9780374533557', 499,
        'Non-Fiction', 0, GETDATE(), GETDATE());

-- Note on Availability values:
-- 0 = Available
-- 1 = Borrowed
-- 2 = Reserved
-- 3 = Maintenance
-- All books start as Available (0) and will be updated by loan operations
