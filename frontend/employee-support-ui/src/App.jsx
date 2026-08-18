import { BrowserRouter, Routes, Route, Outlet } from "react-router-dom";
import Sidebar from "./Components/Sidebar";
import Header from "./Components/Header";
import ProtectedRoute from "./Rotues/ProtectedRoute";
import RoleGuard from "./Rotues/RoleGuard";
import ToastView from "./Components/Toast";
import Login from "./Pages/Login";
import Dashboard from "./Pages/Dashboard";
import ChatWindow from "./Pages/ChatWindow";
import ApplyLeave from "./Pages/ApplyLeave";
import MyLeaves from "./Pages/MyLeaves";
import MyPayslips from "./Pages/MyPayslips";
import MyTickets from "./Pages/MyTickets";
import MyReimbursements from "./Pages/MyReimbursements";
import Profile from "./Pages/Profile";
import ChangePassword from "./Pages/ChangePassword";
import AdminApprovals from "./Pages/AdminApprovals";
import AdminEmployees from "./Pages/AdminEmployees";
import AdminTickets from "./Pages/AdminTickets";

function AppShell() {
  return (
    <div className="app-shell">
      <Sidebar />
      <div className="main-area">
        <Header />
        <Outlet />
      </div>
      <ToastView />
    </div>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route
          element={
            <ProtectedRoute>
              <AppShell />
            </ProtectedRoute>
          }
        >
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/chat" element={<ChatWindow />} />
          <Route path="/leaves" element={<ApplyLeave />} />
          <Route path="/my-leaves" element={<MyLeaves />} />
          <Route path="/payslips" element={<MyPayslips />} />
          <Route path="/tickets" element={<MyTickets />} />
          <Route path="/reimbursements" element={<MyReimbursements />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/profile/change-password" element={<ChangePassword />} />

          {/* Manager / HR / Admin */}
          <Route
            element={
              <RoleGuard allowedRoles={["Manager", "HR", "Admin"]} />
            }
          >
            <Route path="/admin/approvals" element={<AdminApprovals />} />
          </Route>

          {/* HR / Admin */}
          <Route
            element={
              <RoleGuard allowedRoles={["HR", "Admin"]} />
            }
          >
            <Route path="/admin/employees" element={<AdminEmployees />} />
            <Route path="/admin/tickets" element={<AdminTickets />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
