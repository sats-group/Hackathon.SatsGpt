# SATS Assistant – Hackathon Presentation

## ✨ Motivation

At SATS, we often get stuck mid-task or during meetings asking:

* “How does this work again?”
* “Who should I talk to about this?”

This friction especially affects:

* New hires (e.g. developers setting up devices and apps)
* Internal promotions (e.g. new managers learning routines)
* Cross-team collaboration

**Example:**
When onboarding a new developer, we must:

* Know the process for ordering equipment
* Understand what systems (Quinyx, Findity, Yoobic, Slack, etc.) must be installed
* Learn routines for cloning, committing, deploying
* Follow naming and code conventions

All of this is scattered, tribal, or hard to find.

---

## 🧹 The Vision: A Knowledge Assistant for SATS

### Goal

Create a virtual assistant that:

* Understands SATS policies, people, and procedures
* Helps internal users find answers fast
* Can read, search, and **maintain** our knowledge base
* Allows users to contribute and update content conversationally

---

## 🌐 How It Works

### Knowledge Base

* Stored in a **PostgreSQL database** with **ltree-based paths**
* Organized into a **virtual folder system**
* All documents are vector-embedded for **semantic search**

### Assistant Capabilities

* ✅ Search documents semantically (vector search)
* ✅ Navigate directory structure (folders)
* ✅ Read document summaries or full content
* ✅ Add, update, and delete documents (with write access)

---

## 🚀 Impact

* ✉️ Shortens time-to-knowledge
* 🤔 Reduces interruption from unclear processes
* 🔗 Connects people to the right contacts faster
* 🧳 Onboards new employees more smoothly
* ✨ Decentralizes and **democratizes** internal knowledge

---

## 🎓 Takeaway

**This assistant is more than a search box — it’s a SATS-native coworker.**

Let’s build a prototype that shows how we can:

* Answer real questions
* Learn from user input
* Improve how we work

—

> “If only someone could just tell me how this works...”

**Now it can.**
