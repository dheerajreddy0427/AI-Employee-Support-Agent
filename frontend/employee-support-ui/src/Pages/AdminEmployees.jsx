import { useEffect, useState } from "react";
import { getAllEmployees, createEmployee, updateEmployee } from "../Services/employeeApi";
import EmptyState from "../Components/EmptyState";
import { toast } from "../Components/Toast";

const ROLES = ["Employee", "Manager", "HR", "Admin"];
const EMPTY_FORM = {
  fullName: "",
  email: "",
  department: "",
  employeeCode: "",
  leaveBalance: 20,
  role: "Employee"
};

export default function AdminEmployees() {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({});
  const [creating, setCreating] = useState(false);
  const [createForm, setCreateForm] = useState(EMPTY_FORM);

  const load = async () => {
    setLoading(true);
    try {
      const data = await getAllEmployees();
      setEmployees(data || []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const startEdit = (emp) => {
    setEditingId(emp.employeeId);
    setEditForm({
      fullName: emp.fullName,
      email: emp.email,
      department: emp.department,
      leaveBalance: emp.leaveBalance,
      role: emp.role,
      employeeCode: emp.employeeCode || ""
    });
  };

  const saveEdit = async () => {
    try {
      await updateEmployee(editingId, editForm);
      toast.success(`Employee #${editingId} updated.`);
      setEditingId(null);
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.error || "Failed to update employee.");
    }
  };

  const submitCreate = async (e) => {
    e.preventDefault();
    try {
      await createEmployee(createForm);
      toast.success("Employee created.");
      setCreateForm(EMPTY_FORM);
      setCreating(false);
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.error || "Failed to create employee.");
    }
  };

  return (
    <div className="page">
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 12 }}>
        <div>
          <h1 className="page-title">Employees</h1>
          <p className="page-subtitle">Create, edit, and manage team members.</p>
        </div>
        <button className="btn btn-primary-solid" onClick={() => setCreating((c) => !c)}>
          {creating ? "Cancel" : "+ New employee"}
        </button>
      </div>

      {creating && (
        <div className="card" style={{ marginBottom: 16 }}>
          <div className="card-header">
            <div className="card-title">Create employee</div>
          </div>
          <form className="form" onSubmit={submitCreate}>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <div className="field">
                <label>Full name</label>
                <input className="input" required value={createForm.fullName}
                  onChange={(e) => setCreateForm({ ...createForm, fullName: e.target.value })} />
              </div>
              <div className="field">
                <label>Email</label>
                <input className="input" type="email" required value={createForm.email}
                  onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })} />
              </div>
              <div className="field">
                <label>Department</label>
                <input className="input" required value={createForm.department}
                  onChange={(e) => setCreateForm({ ...createForm, department: e.target.value })} />
              </div>
              <div className="field">
                <label>Employee code</label>
                <input className="input" required value={createForm.employeeCode}
                  onChange={(e) => setCreateForm({ ...createForm, employeeCode: e.target.value })} />
              </div>
              <div className="field">
                <label>Leave balance</label>
                <input className="input" type="number" min="0" value={createForm.leaveBalance}
                  onChange={(e) => setCreateForm({ ...createForm, leaveBalance: Number(e.target.value) })} />
              </div>
              <div className="field">
                <label>Role</label>
                <select className="input" value={createForm.role}
                  onChange={(e) => setCreateForm({ ...createForm, role: e.target.value })}>
                  {ROLES.map((r) => <option key={r}>{r}</option>)}
                </select>
              </div>
            </div>
            <button type="submit" className="btn btn-primary-solid">Create</button>
          </form>
        </div>
      )}

      <div className="card">
        {loading ? (
          <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
            <span className="spinner lg" />
          </div>
        ) : employees.length === 0 ? (
          <EmptyState icon="👥" title="No employees" />
        ) : (
          <div className="table-wrap" style={{ border: "none" }}>
            <table className="table">
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>Role</th>
                  <th>Leave balance</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {employees.map((e) => (
                  <tr key={e.employeeId}>
                    {editingId === e.employeeId ? (
                      <>
                        <td>
                          <input className="input" style={{ minWidth: 90 }}
                            value={editForm.employeeCode}
                            onChange={(ev) => setEditForm({ ...editForm, employeeCode: ev.target.value })} />
                        </td>
                        <td>
                          <input className="input" value={editForm.fullName}
                            onChange={(ev) => setEditForm({ ...editForm, fullName: ev.target.value })} />
                        </td>
                        <td>
                          <input className="input" type="email" value={editForm.email}
                            onChange={(ev) => setEditForm({ ...editForm, email: ev.target.value })} />
                        </td>
                        <td>
                          <input className="input" value={editForm.department}
                            onChange={(ev) => setEditForm({ ...editForm, department: ev.target.value })} />
                        </td>
                        <td>
                          <select className="input" value={editForm.role}
                            onChange={(ev) => setEditForm({ ...editForm, role: ev.target.value })}>
                            {ROLES.map((r) => <option key={r}>{r}</option>)}
                          </select>
                        </td>
                        <td>
                          <input className="input" type="number" min="0" value={editForm.leaveBalance}
                            onChange={(ev) => setEditForm({ ...editForm, leaveBalance: Number(ev.target.value) })} />
                        </td>
                        <td>
                          <div style={{ display: "flex", gap: 6 }}>
                            <button className="btn btn-soft" onClick={saveEdit}>Save</button>
                            <button className="btn btn-ghost" onClick={() => setEditingId(null)}>Cancel</button>
                          </div>
                        </td>
                      </>
                    ) : (
                      <>
                        <td style={{ color: "var(--text-soft)" }}>{e.employeeCode || "—"}</td>
                        <td style={{ fontWeight: 600 }}>{e.fullName}</td>
                        <td style={{ color: "var(--text-muted)" }}>{e.email}</td>
                        <td>{e.department}</td>
                        <td><span className="badge primary">{e.role}</span></td>
                        <td>{e.leaveBalance}</td>
                        <td>
                          <button className="btn btn-soft" onClick={() => startEdit(e)}>Edit</button>
                        </td>
                      </>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
