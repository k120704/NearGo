import type { Metadata } from "next"
import { Geist, Geist_Mono } from "next/font/google"
import "./globals.css"

const geistSans = Geist({
  subsets: ["latin"],
  variable: "--font-geist-sans",
})

const geistMono = Geist_Mono({
  subsets: ["latin"],
  variable: "--font-geist-mono",
})

export const metadata: Metadata = {
  title: "NearGo — Thực phẩm cận hạn, giá tốt, chống lãng phí",
  description:
    "NearGo là sàn thương mại điện tử bán thực phẩm cận hạn sử dụng từ các siêu thị uy tín với giá giảm sâu, giúp tiết kiệm chi phí và giảm lãng phí thực phẩm.",
  generator: "v0.app",
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html lang="vi" className={`${geistSans.variable} ${geistMono.variable} bg-background`}>
      <body className="font-sans antialiased">{children}</body>
    </html>
  )
}
