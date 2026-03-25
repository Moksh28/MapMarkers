-- Run this script in your SQL Server database before starting the app

CREATE TABLE Markers (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(100)  NOT NULL,
    Description NVARCHAR(500)  NULL,
    Latitude    FLOAT          NOT NULL,
    Longitude   FLOAT          NOT NULL,
    Color       NVARCHAR(20)   NOT NULL DEFAULT 'red',
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

-- Seed 8 default markers (world landmarks)
INSERT INTO Markers (Title, Description, Latitude, Longitude, Color) VALUES
('Eiffel Tower',         'Iconic iron lattice tower in Paris, France.',           48.8584,   2.2945,   'red'),
('Statue of Liberty',    'Colossal neoclassical sculpture on Liberty Island.',    40.6892,  -74.0445,  'blue'),
('Big Ben',              'Clock tower at the Palace of Westminster, London.',     51.5007,  -0.1246,   'green'),
('Sydney Opera House',   'Multi-venue performing arts centre in Sydney.',        -33.8568,  151.2153,  'yellow'),
('Taj Mahal',            'Ivory-white marble mausoleum in Agra, India.',          27.1751,   78.0421,  'purple'),
('Colosseum',            'Oval amphitheatre in the centre of Rome, Italy.',       41.8902,   12.4922,  'red'),
('Machu Picchu',         'Incan citadel set high in the Andes Mountains.',       -13.1631,  -72.5450,  'orange'),
('Great Wall of China',  'Series of fortifications along the northern borders.',  40.4319,  116.5704,  'blue');
