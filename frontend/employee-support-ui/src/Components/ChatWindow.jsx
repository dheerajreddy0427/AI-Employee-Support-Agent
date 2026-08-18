import { useState, useEffect, useRef } from "react";
import { sendMessage } from "../services/agentApi";
import {
  getChatHistory,
  saveChatMessage
} from "../services/chatApi";
import { getEmployeeId } from "../utils/jwtHelper";

import MessageBubble from "./MessageBubble";

function ChatWindow({ quickMessage }) {

  const bottomRef = useRef(null);

  const employeeId = getEmployeeId();

  const [message, setMessage] = useState("");

  const [typing, setTyping] = useState(false);

  const [sending, setSending] = useState(false);

  const [messages, setMessages] = useState([
    {
      sender: "Agent",
      text: "Hello! How can I help you today?",
      time: new Date().toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit"
      })
    }
  ]);

  // Load chat history
  useEffect(() => {
    loadHistory();
  }, []);

  // Scroll to bottom
  useEffect(() => {
    bottomRef.current?.scrollIntoView({
      behavior: "smooth"
    });
  }, [messages, typing]);

  // Fill input when sidebar button is clicked
  useEffect(() => {
    if (quickMessage) {
      setMessage(quickMessage);
    }
  }, [quickMessage]);

  const loadHistory = async () => {

    try {

      if (!employeeId) return;

      const historyData = await getChatHistory(employeeId);

      const formatted = historyData.map((msg) => ({
        sender:
          msg.sender === "User"
            ? "You"
            : "Agent",

        text: msg.messageText,

        time: new Date(msg.createdAt).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit"
        })
      }));

      if (formatted.length > 0) {
        setMessages(formatted);
      }

    } catch (error) {

      console.error(
        "Failed to load chat history",
        error
      );

    }

  };

  const handleSend = async () => {

    if (!message.trim()) return;

    if (sending) return;

    setSending(true);

    const userMessage = {
      sender: "You",
      text: message,
      time: new Date().toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit"
      })
    };

    setMessages((prev) => [...prev, userMessage]);

    try {

      await saveChatMessage({
        employeeId,
        sender: "User",
        messageText: message
      });

      setTyping(true);

      const result = await sendMessage(message);

      await saveChatMessage({
        employeeId,
        sender: "Agent",
        messageText: result.response
      });

      setMessages((prev) => [
        ...prev,
        {
          sender: "Agent",
          text: result.response,
          time: new Date().toLocaleTimeString([], {
            hour: "2-digit",
            minute: "2-digit"
          })
        }
      ]);

    } catch (error) {

      console.error(error);

      setMessages((prev) => [
        ...prev,
        {
          sender: "Agent",
          text: "Unable to connect to backend.",
          time: new Date().toLocaleTimeString([], {
            hour: "2-digit",
            minute: "2-digit"
          })
        }
      ]);

    } finally {

      setTyping(false);
      setSending(false);
      setMessage("");

    }

  };

  return (
   <div className="main-layout">
    <div className="chat-section">

      <div className="chat-header">
        Employee Assistant
      </div>

      <div className="messages-container">

        {messages.map((msg, index) => (

          <MessageBubble
            key={index}
            sender={msg.sender}
            text={msg.text}
            time={msg.time}
          />

        ))}

        {typing && (

          <div className="typing">
            <span></span>
            <span></span>
            <span></span>
          </div>

        )}

        <div ref={bottomRef}></div>

      </div>

      <div className="input-container">

        <input
          type="text"
          value={message}
          placeholder="Ask about leave, payslip, reimbursement..."
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={(e) => {

            if (e.key === "Enter") {
              handleSend();
            }

          }}
        />

        <button
          onClick={handleSend}
          disabled={sending}
        >
          {sending ? "Sending..." : "Send"}
        </button>

      </div>

    </div>
    </div>

  );

}

export default ChatWindow;