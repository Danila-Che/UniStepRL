from agent_model import AgentModel, save_pt_model, save_onnx_model

def main():
    model = AgentModel(5, 5)
    pt_path = "simple_environment_agent_model.pt2"
    onnx_path = "simple_environment_agent_model.onnx"
    
    save_pt_model(model, pt_path)
    save_onnx_model(model, onnx_path)

if __name__ == "__main__":
    main()
