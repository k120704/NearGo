import {
  pgTable,
  text,
  timestamp,
  boolean,
  serial,
  integer,
  doublePrecision,
} from "drizzle-orm/pg-core"

// --- Better Auth required tables -------------------------------------------
// Column names are camelCase to match Better Auth's defaults. Do not rename.

export const user = pgTable("user", {
  id: text("id").primaryKey(),
  name: text("name").notNull(),
  email: text("email").notNull().unique(),
  emailVerified: boolean("emailVerified").notNull().default(false),
  image: text("image"),
  // NearGo role: "customer" | "supermarket" | "admin"
  role: text("role").notNull().default("customer"),
  phone: text("phone"),
  address: text("address"),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
  updatedAt: timestamp("updatedAt").notNull().defaultNow(),
})

export const session = pgTable("session", {
  id: text("id").primaryKey(),
  expiresAt: timestamp("expiresAt").notNull(),
  token: text("token").notNull().unique(),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
  updatedAt: timestamp("updatedAt").notNull().defaultNow(),
  ipAddress: text("ipAddress"),
  userAgent: text("userAgent"),
  userId: text("userId")
    .notNull()
    .references(() => user.id, { onDelete: "cascade" }),
})

export const account = pgTable("account", {
  id: text("id").primaryKey(),
  accountId: text("accountId").notNull(),
  providerId: text("providerId").notNull(),
  userId: text("userId")
    .notNull()
    .references(() => user.id, { onDelete: "cascade" }),
  accessToken: text("accessToken"),
  refreshToken: text("refreshToken"),
  idToken: text("idToken"),
  accessTokenExpiresAt: timestamp("accessTokenExpiresAt"),
  refreshTokenExpiresAt: timestamp("refreshTokenExpiresAt"),
  scope: text("scope"),
  password: text("password"),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
  updatedAt: timestamp("updatedAt").notNull().defaultNow(),
})

export const verification = pgTable("verification", {
  id: text("id").primaryKey(),
  identifier: text("identifier").notNull(),
  value: text("value").notNull(),
  expiresAt: timestamp("expiresAt").notNull(),
  createdAt: timestamp("createdAt").defaultNow(),
  updatedAt: timestamp("updatedAt").defaultNow(),
})

// --- NearGo app tables -----------------------------------------------------

export const categories = pgTable("categories", {
  id: serial("id").primaryKey(),
  name: text("name").notNull(),
  slug: text("slug").notNull().unique(),
  description: text("description"),
  imageUrl: text("imageUrl"),
  sortOrder: integer("sortOrder").notNull().default(0),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
})

export const supermarkets = pgTable("supermarkets", {
  id: serial("id").primaryKey(),
  name: text("name").notNull(),
  slug: text("slug").notNull().unique(),
  description: text("description"),
  logoUrl: text("logoUrl"),
  coverUrl: text("coverUrl"),
  address: text("address"),
  city: text("city"),
  phone: text("phone"),
  rating: doublePrecision("rating").notNull().default(0),
  isVerified: boolean("isVerified").notNull().default(false),
  // owner user id (the supermarket seller account)
  ownerId: text("ownerId"),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
})

export const products = pgTable("products", {
  id: serial("id").primaryKey(),
  name: text("name").notNull(),
  slug: text("slug").notNull(),
  description: text("description"),
  imageUrl: text("imageUrl"),
  // pricing
  originalPrice: doublePrecision("originalPrice").notNull(),
  salePrice: doublePrecision("salePrice").notNull(),
  unit: text("unit").notNull().default("sản phẩm"),
  stock: integer("stock").notNull().default(0),
  // near-expiry info
  expiryDate: timestamp("expiryDate"),
  isActive: boolean("isActive").notNull().default(true),
  categoryId: integer("categoryId").notNull(),
  supermarketId: integer("supermarketId").notNull(),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
})

export const cartItems = pgTable("cart_items", {
  id: serial("id").primaryKey(),
  userId: text("userId").notNull(),
  productId: integer("productId").notNull(),
  quantity: integer("quantity").notNull().default(1),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
})

export const orders = pgTable("orders", {
  id: serial("id").primaryKey(),
  userId: text("userId").notNull(),
  orderCode: text("orderCode").notNull(),
  status: text("status").notNull().default("pending"),
  totalAmount: doublePrecision("totalAmount").notNull(),
  shippingAddress: text("shippingAddress"),
  phone: text("phone"),
  note: text("note"),
  paymentMethod: text("paymentMethod").notNull().default("cod"),
  createdAt: timestamp("createdAt").notNull().defaultNow(),
})

export const orderItems = pgTable("order_items", {
  id: serial("id").primaryKey(),
  orderId: integer("orderId").notNull(),
  productId: integer("productId").notNull(),
  productName: text("productName").notNull(),
  productImage: text("productImage"),
  price: doublePrecision("price").notNull(),
  quantity: integer("quantity").notNull(),
})
