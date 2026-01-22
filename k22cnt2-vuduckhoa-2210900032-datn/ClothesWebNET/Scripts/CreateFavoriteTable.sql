-- Script để tạo bảng Favorite trong database
-- Chạy script này trong SQL Server Management Studio hoặc SQL Server

USE webthoitrang;
GO

-- Tạo bảng Favorite
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Favorite]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Favorite](
        [idUser] [nvarchar](255) NOT NULL,
        [idProduct] [nvarchar](255) NOT NULL,
        [createdAt] [datetime] NULL,
        CONSTRAINT [PK_Favorite] PRIMARY KEY CLUSTERED 
        (
            [idUser] ASC,
            [idProduct] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
    
    -- Thêm Foreign Key constraints
    ALTER TABLE [dbo].[Favorite] WITH CHECK ADD CONSTRAINT [FK_Favorite_User] 
        FOREIGN KEY([idUser]) REFERENCES [dbo].[User] ([idUser])
    
    ALTER TABLE [dbo].[Favorite] CHECK CONSTRAINT [FK_Favorite_User]
    
    ALTER TABLE [dbo].[Favorite] WITH CHECK ADD CONSTRAINT [FK_Favorite_Product] 
        FOREIGN KEY([idProduct]) REFERENCES [dbo].[Product] ([idProduct])
    
    ALTER TABLE [dbo].[Favorite] CHECK CONSTRAINT [FK_Favorite_Product]
    
    PRINT 'Bảng Favorite đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Bảng Favorite đã tồn tại!'
END
GO

